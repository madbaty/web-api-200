using ImTools;
using JasperFx.Events.Projections;
using Marten;
using SoftwareShared.Messages;
using Vendors.Api.Vendors;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Nats;

var builder = WebApplication.CreateBuilder(args);

builder.UseWolverine(opts =>
{
    opts.Policies.UseDurableLocalQueues();
    opts.UseNats(builder.Configuration.GetConnectionString("nats") ??
                        throw new Exception("No NATS connection string configured"))
            .AutoProvision()
            .UseJetStream(js =>
            {
                js.MaxDeliver = 5;
                js.AckWait = TimeSpan.FromSeconds(30);
            })

            .DefineStream("VENDORS", stream =>
                stream.WithSubject("vendors.>")
                    .WithLimits(maxMessages: 5_000, maxAge: TimeSpan.FromDays(5))
                    //.WithReplicas(3)
                    .EnableScheduledDelivery());


    opts.PublishMessage<Vendor>().ToNatsSubject("vendors.add");
    opts.PublishMessage<VendorTombStone>().ToNatsSubject("vendors.tombstone");

});

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("vendors-db");
builder.Services.AddValidation();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddMarten(config =>
{
    config.Projections.Add<VendorListProjection>(ProjectionLifecycle.Async);
    // this is "expensive" but absolutely no "eventual consistency" at all. 
}).IntegrateWithWolverine()
    .UseNpgsqlDataSource()
    .AddAsyncDaemon(JasperFx.Events.Daemon.DaemonMode.Solo)
    .UseLightweightSessions();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

