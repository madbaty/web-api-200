using ApiOne.Endpoints;
using Marten;
using Messages;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Nats;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("db-one");

builder.UseWolverine(options =>
    {
        options.UseNats(builder.Configuration.GetConnectionString("nats") ??
                        throw new Exception("No NATS connection string configured"))
            .AutoProvision()
            .UseJetStream(js =>
            {
                js.MaxDeliver = 5;
                js.AckWait = TimeSpan.FromSeconds(30);
            })
  
            .DefineStream("PEOPLE", stream =>
                stream.WithSubject("people.>")
                    .WithLimits(maxMessages: 5_000, maxAge: TimeSpan.FromDays(5))
                    //.WithReplicas(3)
                    .EnableScheduledDelivery());
    

              
    options.PublishMessage<SendMessage>()
        .ToNatsSubject("messages-sent");
    
    options.PublishMessage<UserDocument>()
        .ToNatsSubject("people.created");
    options.PublishMessage<UserNameChanged>()
        .ToNatsSubject("people.name-changed");
    
    options.PublishMessage<AddThem>()
        .ToNatsSubject("math.add");

});

builder.Services.AddMarten(config =>
    {

    }).UseLightweightSessions()
    .IntegrateWithWolverine()
    .UseNpgsqlDataSource();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapApiOneEndpoints();
app.MapDefaultEndpoints();
app.Run();