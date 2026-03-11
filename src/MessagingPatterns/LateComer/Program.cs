using Messages;
using Wolverine;
using Wolverine.Nats;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
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
   
    // While this will sort of work, you should have exactly one handler for request-response
    // semantics - wolverine will take it, but log an error.
    // options.ListenToNatsSubject("math.add")
    //     .ProcessInline();
    
    options.ListenToNatsSubject("people.>")
        .UseJetStream("PEOPLE", "late-comer");
});
var app = builder.Build();


app.MapDefaultEndpoints();

app.Run();

public static class MessageHandler
{
    public static void Handle(SendMessage message, ILogger logger)
    {
        logger.LogInformation($"Received message: {message.Message}");
    }
    

    public static void Handle(UserDocument user, ILogger logger)
    {
        logger.LogInformation($"Received user: {user.Id} - {user.Name}");
    }
    
    public static void Handle(UserNameChanged change, ILogger logger)
    {
        logger.LogInformation($"User name changed: {change.Id} - {change.NewName}");
    }
    public static NumbersAdded Handle(AddThem request)
    {
        return new NumbersAdded(42);
    }
    
}