using Messages;
using Wolverine;

namespace ApiOne.Endpoints;

public static class Extensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapApiOneEndpoints()
        {
            endpoints.MapPost("/messages", async (SendMessage request, IMessageBus messageBus) =>
            {
                await messageBus.PublishAsync(request);
                return Results.Accepted();
            });

            endpoints.MapPost("/math", async (AddThem request, IMessageBus bus) =>
            {
                // pub sub - probably not a good idea... but for completeness. 
                var result = await bus.InvokeAsync<NumbersAdded>(request);
                return Results.Ok(result);
            });
            
            endpoints.MapPost("/users", async (UserCreate user, IMessageBus bus) =>
            {
                var doc = new UserDocument(Guid.NewGuid(), user.Name);
                await bus.PublishAsync(doc);
                return Results.Accepted();
            });
            endpoints.MapPut("/users/{id:guid}/name", async (Guid id, UserCreate user, IMessageBus bus) =>
            {
                var nameChanged = new UserNameChanged(id, user.Name);
                await bus.PublishAsync(nameChanged);
                return Results.Accepted();
            });

            return endpoints;
        }
    }
}

public record UserCreate(string Name);


