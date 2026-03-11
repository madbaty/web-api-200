using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Vendors.Api.Vendors;

/* NOTE
 * This will not be feature complete - just enough so you "get it"
 */

[ApiController]
public class Controller(IDocumentSession session) : ControllerBase
{
    // POST

    [HttpPost("/vendors")]
    public async Task<ActionResult> AddVendorAsync(
    [FromBody] CreateVendorRequestModel request
    )
    {
        var vendorId = Guid.NewGuid();
        session.Events.StartStream(
            vendorId,
            new VendorCreated(request.Name, request.Url),
            new PointOfContactAssignedToAVendor(request.PointOfContact.Name, request.PointOfContact.Email, request.PointOfContact.Phone)
            );
        await session.SaveChangesAsync();
        return Created($"/vendors/{vendorId}", new { });
    }

   
    [HttpPut("/vendors/{id:guid}/point-of-contact")]
    public async Task<ActionResult> UpdatePoc(Guid id, [FromBody] VendorPointOfContactModel request)
    {
        session.Events.Append(id, new PointOfContactAssignedToAVendor(request.Name, request.Email, request.Phone));
        await session.SaveChangesAsync();
        return Ok();
    }
   
    [HttpGet("/vendors")]
    public async Task<ActionResult> GetAllVendorsAsync(CancellationToken token)
    {
        return Ok();
    }

    [HttpGet("/vendors/{id:guid}")]
    public async Task<ActionResult> GetVendorByIdAsync(Guid id, CancellationToken token)
    {
        var vendor = await session.Events.AggregateStreamAsync<VendorDetailsReadModel>(id);
        if (vendor == null)
            return NotFound();

        return Ok(vendor);
    }
}

public record VendorCreated(string Name, string Url);
public record PointOfContactAssignedToAVendor(string Name, string Email, string Phone);