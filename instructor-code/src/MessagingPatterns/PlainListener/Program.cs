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
    
    options.ListenToNatsSubject("people.created") // people.>
        .UseJetStream("PEOPLE", "plain-listener");
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
    
}
