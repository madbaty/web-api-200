using Software.Api.Clients;
using SoftwareShared.Notifications;

namespace Software.Api;

public static class NotificationHandler
{
    public static async Task Handle(NotificationRequest command, IDoNotifications api)
    {
        await api.SendNotification(command);
    }
}
