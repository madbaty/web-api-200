using Marten;

namespace Software.Api.Vendors;

public class NotificationBackgroundWorker(IServiceProvider sp) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Timer(DoTheNotification, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void DoTheNotification(object? state)
    {
        using var scope = sp.CreateScope();

        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationBackgroundWorker>>();

        logger.LogInformation("Doing some work, yo");
    }
}
