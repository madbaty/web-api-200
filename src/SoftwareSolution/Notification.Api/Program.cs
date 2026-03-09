using SoftwareShared.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/notifications", async (NotificationRequest request) =>
{
    await Task.Delay(1200); // don't do this. 
    // do some real work.
    // This is ganky - promise I'll show this better this afternoon.
    app.Logger.LogInformation("Notifying folks of " + request.Message);
    return TypedResults.Ok();
});


app.Run();

