using JasperFx.CommandLine.Descriptions;
using Marten;
using Software.Api.CatalogItems;
using Software.Api.Clients;
using Software.Api.Vendors;


var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDataSource("software-db"); 
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


}).UseLightweightSessions().UseNpgsqlDataSource();


builder.Services.AddHttpClient<NotificationsApi>(client =>
{
    // we prefer https, but we'll http if that is availble, and get the address for that api.
    client.BaseAddress = new Uri("https+http://notification-api");
});

builder.Services.Configure<BlockedVendorsOptions>(
    builder.Configuration.GetSection("BlockedVendors")
    );

builder.Services.AddScoped<IDoNotifications>(sp => sp.GetRequiredService<NotificationsApi>());
builder.Services.AddScoped<VendorExistsFilter>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapCatalogItemRoutes();

app.MapDefaultEndpoints(); // this comes from service defaults, and this is mostly health checks.
app.Run();
