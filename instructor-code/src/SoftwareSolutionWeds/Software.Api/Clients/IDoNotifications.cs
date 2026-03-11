using SoftwareShared.Notifications;

namespace Software.Api.Clients;

public interface IDoNotifications
{
    Task SendNotification(NotificationRequest request);
}