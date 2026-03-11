# Vendors API — Concepts Overview

## Controller Structure

`VendorController` is an ASP.NET Core API controller that uses **primary constructor injection** to receive an `IDocumentSession` (Marten) directly in the constructor declaration, eliminating the need for a backing field and explicit constructor body.

```csharp
public class VendorController(IDocumentSession session) : ControllerBase
```

---

## Routing

Each action method is decorated with an HTTP verb attribute that also defines the route:

| Attribute | Route | Description |
|---|---|---|
| `[HttpPost]` | `/vendors` | Create a new vendor |
| `[HttpGet]` | `/vendors` | List all vendors |
| `[HttpGet]` | `/vendors/{id:guid}` | Get a vendor by ID |
| `[HttpPut]` | `/vendors/{id:guid}/point-of-contact` | Update a vendor's point of contact |

Route constraints (e.g., `{id:guid}`) are used to enforce the expected type of a route parameter at the routing layer, before the action method is even reached.

---

## Authorization

Authorization is applied per-endpoint using the `[Authorize]` attribute.

- `[Authorize]` — requires any authenticated user (a valid JWT from a trusted authority).
- `[Authorize(Policy = "SoftwareCenterManager")]` — requires the caller to satisfy a named policy, enabling role- or claim-based access control beyond simple authentication.

Endpoints without an `[Authorize]` attribute are publicly accessible.

---

## Method Injection with `[FromServices]`

Some dependencies are only needed by a single action and don't belong on the constructor. `[FromServices]` injects them directly into the method parameter list:

```csharp
public async Task<ActionResult> AddVendorAsync(
    [FromBody] CreateVendorRequestModel request,
    [FromServices] IDoNotifications api,
    [FromServices] TimeProvider clock)
```

This keeps the controller's constructor lean and makes each action's dependencies explicit.

---

## Input Validation

Business rule validation (beyond model validation) is done inline before any persistence logic:

```csharp
if (request.Name.Trim().ToLower() == "oracle")
{
    return BadRequest("We are not allowed to do business with them");
}
```

Returning `BadRequest(...)` produces a `400` response with a message body.

---

## Entity Mapping Methods

Mapping between request/response models and the database entity is encapsulated directly on `VendorEntity` using static factory methods and instance projection methods:

- `VendorEntity.From(request, clock)` — creates a new entity from a `CreateVendorRequestModel`, keeping construction logic off the controller.
- `entity.ToDetails()` — projects the entity to a `VendorDetailsModel` (full representation).
- `entity.ToSummary()` — projects the entity to a `VendorSummaryModel` (lightweight list representation).

This pattern keeps the controller free of mapping noise and gives a single place to update if model shapes change.

---

## Marten Document Session

Marten is used as the persistence layer, treating PostgreSQL as a document database.

- `session.Store(entity)` — stages an entity for insert or update.
- `session.SaveChangesAsync()` — flushes all staged changes to the database in a single transaction.
- `session.Query<T>()` — provides a LINQ-queryable interface over stored documents.
- `session.LoadAsync<T>(id)` — loads a single document by its ID, returning `null` if not found.

---

## Cross-Service Communication

After persisting a new vendor, a notification is sent to a remote service via `IDoNotifications`:

```csharp
await api.SendNotification(new NotificationRequest { Message = "New vendor added " + request.Name });
await session.SaveChangesAsync();
```

Note the ordering: the notification is dispatched **before** `SaveChangesAsync`. This is intentional — distributed transactions across service boundaries are not possible, so the design acknowledges that these are two independent operations. Consider at-least-once delivery patterns (e.g., outbox pattern) if strong consistency is required.

---

## HTTP Response Conventions

| Method | Success Response | Notes |
|---|---|---|
| `AddVendorAsync` | `201 Created` | Includes a `Location`-style URI and the created resource body |
| `GetAllVendorsAsync` | `200 OK` | Returns a list of summary models |
| `GetVendorByIdAsync` | `200 OK` / `404 Not Found` | Returns full detail or not found |
| `UpdatePoc` | `204 No Content` | Returns no body on success |

`Created(uri, body)` sets the response status to `201` and includes the resource in the body. `NoContent()` returns `204` with no body, appropriate for updates where the client already has the representation.

---

## Cancellation Tokens

### How ASP.NET Core provides them

ASP.NET Core automatically binds a `CancellationToken` to any action method parameter of that type. The token is tied to the HTTP request lifecycle — it is cancelled when the client disconnects, the request times out, or the server is shutting down. No extra configuration is required; just declare the parameter and the framework handles the rest.

```csharp
public async Task<ActionResult> GetAllVendorsAsync(CancellationToken token)
```

### Use them for reads

Pass the token through to any async read operation — database queries, HTTP calls to other services, file reads, etc. If the client is gone, there is no point completing the work, so cancelling early frees up threads, database connections, and other resources.

```csharp
var allVendors = await session.Query<VendorEntity>().ToListAsync(token);
var vendor = await session.LoadAsync<VendorEntity>(id, token);
```

### Do NOT use them for writes

Do not pass the cancellation token to `SaveChangesAsync()` or any other operation that mutates state. By the time you reach a write, the application has already validated the request and made the decision to commit. Cancelling mid-write because a client disconnected can leave data in an inconsistent state and discard legitimate work the server should complete regardless.

```csharp
session.Store(entityToSave);
await session.SaveChangesAsync(); // no token — let this complete
```

The same principle applies to outbound calls that trigger side effects, such as sending notifications or publishing events. If you've decided to send it, see it through.

### Passing `CancellationToken.None` explicitly

In cases where you want to be explicit about intentionally not supporting cancellation, you can pass `CancellationToken.None` rather than omitting the parameter. This makes the intent clear to future readers of the code:

```csharp
await session.SaveChangesAsync(CancellationToken.None);
```

### Summary

| Operation type | Pass the token? | Reason |
|---|---|---|
| Database reads | ✅ Yes | No value completing a query for a disconnected client |
| HTTP calls (reads) | ✅ Yes | Frees upstream resources if the caller is gone |
| Database writes | ❌ No | Must complete to avoid inconsistent state |
| Notification / event dispatch | ❌ No | Side effects should not be abandoned mid-flight |

