using Marten;
using Messages;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Nats;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("db-two");

builder.UseWolverine(options =>
{
    options.UseNats(builder.Configuration.GetConnectionString("nats") ??
                    throw new Exception("No NATS connection string configured"))
        .AutoProvision()
        .UseJetStream(js =>
        {
            js.MaxDeliver = 5;
            js.AckWait = TimeSpan.FromSeconds(30);
        });
        
              
    options.ListenToNatsSubject("messages-sent")
        .BufferedInMemory();
    
    options.ListenToNatsSubject("math.add")
        .ProcessInline();
    
    options.ListenToNatsSubject("people.>") // I want to see any message products.*.available.uk
        .UseJetStream("PEOPLE", "api-two");


});

builder.Services.AddMarten(config =>
    {

    }).UseLightweightSessions()
    .IntegrateWithWolverine()
    .UseNpgsqlDataSource();

var app = builder.Build();

app.MapGet("/users", async (IDocumentSession session) =>
{
    var response = await session.Query<UserDocument>().ToListAsync();
    return Results.Ok(response);
});

app.Run();

public static class MessageHandler
{
    public static void Handle(SendMessage message, ILogger logger)
    {
        logger.LogInformation($"Received message: {message.Message}");
    }
    
    public static NumbersAdded Handle(AddThem request)
    {
        var sum = request.Numbers.Sum();
        return new NumbersAdded(sum);
    }
    public static async Task Handle(UserDocument user, IDocumentSession session)
    {
        session.Store(user);
        await session.SaveChangesAsync();
    }
    public static async Task Handle(UserNameChanged nameChanged, IDocumentSession session)
    {
        var user = await session.LoadAsync<UserDocument>(nameChanged.Id);
        if (user != null)
        {
            var updatedUser = user with { Name = nameChanged.NewName };
            session.Store(updatedUser);
            await session.SaveChangesAsync();
        }
    }
}

