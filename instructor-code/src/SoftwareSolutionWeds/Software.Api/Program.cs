
using ImTools;
using Marten;
using Software.Api.CatalogItems;
using Software.Api.Clients;
using Software.Api.Vendors;
using SoftwareShared.Messages;
using Wolverine;
using Wolverine.Marten;
using Wolverine.Nats;


var builder = WebApplication.CreateBuilder(args);
builder.UseWolverine(opt =>
{
    // talk about this.
    opt.Policies.UseDurableLocalQueues(); // write this to the database.
    opt.UseNats(builder.Configuration.GetConnectionString("nats") ??
                      throw new Exception("No NATS connection string configured"))
          .AutoProvision()
          .UseJetStream(js =>
          {
              js.MaxDeliver = 5;
              js.AckWait = TimeSpan.FromSeconds(30);
          });




    //opts.PublishMessage<Vendor>().ToNatsSubject("vendors.add");
    //opts.PublishMessage<VendorTombStone>().ToNatsSubject("vendors.tombstone");
    opt.ListenToNatsSubject("vendors.add")
    .UseJetStream("VENDORS", "software");

    opt.ListenToNatsSubject("vendors.tombstone")
    .UseJetStream("VENDORS", "software");
});
builder.AddNpgsqlDataSource("software-db"); // use the configuration api to find me the connection string for software-db
builder.Services.AddValidation(); 
builder.AddServiceDefaults(); 

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System); 

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddAuthorizationBuilder().AddPolicy("SoftwareCenterManager", pol =>
{
    // Chaining RequireRole calls enforces AND logic — the user must have BOTH roles.
    // Note: RequireRole("SoftwareCenter", "Manager") would be OR logic (either role suffices).
    pol.RequireRole("SoftwareCenter");
    pol.RequireRole("Manager");
}).AddPolicy("SoftwareCenter", pol =>
{
    pol.RequireRole("SoftwareCenter");
});


builder.Services.AddControllers();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("software-db") ??
    throw new Exception("no connection string");

builder.Services.AddMarten(options =>
{
    options.RegisterDocumentType<Vendor>(); // will just make sure this is created on startup

}).UseLightweightSessions().UseNpgsqlDataSource().IntegrateWithWolverine();


builder.Services.AddHttpClient<NotificationsApi>(client =>
{
    // we prefer https, but we'll http if that is availble, and get the address for that api.
    client.BaseAddress = new Uri("https+http://notification-api"); // "Service Discovery" - 
});

// this makes it so we can inject IOptions<BlockedVendorOptions> in our controllers, services, etc.
builder.Services.Configure<BlockedVendorsOptions>(
    builder.Configuration.GetSection(BlockedVendorsOptions.SectionName)
    );

builder.Services.AddScoped<IDoNotifications>(sp => sp.GetRequiredService<NotificationsApi>());
builder.Services.AddScoped<VendorExistsFilter>(); // If you don't know what that means, ASK OR LOOK IT UP.

builder.Services.AddHostedService<NotificationBackgroundWorker>();
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// using reflection at startup to automatically "discover' all the controllers and build the route table.
// Cannot do this if you are using AOT. (opposite of JIT) 
app.MapControllers();

// I prefer this because I use a lot of feature flags.
app.MapCatalogItemRoutes();

app.MapDefaultEndpoints(); // this comes from service defaults, and this is mostly health checks.
app.Run();


public class VendorHandler
{
    public static async Task Handle(Vendor vendor, IDocumentSession session)
    {
        session.Store<Vendor>();
        await session.SaveChangesAsync();
    }

    public static async Task Handle(VendorTombStone ts, IDocumentSession session)
    {
        session.Delete<Vendor>(ts.Id);
        await session.SaveChangesAsync();
    }
}