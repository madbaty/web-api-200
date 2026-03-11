var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration.AddJsonFile("yarp-config.json", optional: false, reloadOnChange: true).Build();


builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver(); // going to translate software-api to an address


var app = builder.Build();

app.MapDefaultEndpoints();
app.MapReverseProxy();

app.UseHttpsRedirection();


app.Run();

