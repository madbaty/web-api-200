using Marten;

namespace Software.Api.Vendors;

public class NotificationBackgroundWorker(IServiceProvider  sp) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // async - listen to and consume a Channel - and "spring to life" when a message comes in.
        // use a timer.
        new Timer(DoTheNotification, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void DoTheNotification(object? state)
    {
        // check the database for any work. "inbox"
        using var scope = sp.CreateScope();

        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationBackgroundWorker>>();
        // the hard part goes here.
        // HangFire - 
        // Wolverine
        logger.LogInformation("Doing some work, yo.");
        // use that session.

    }
}
