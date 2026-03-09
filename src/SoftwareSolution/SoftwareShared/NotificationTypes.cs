using System;
using System.Collections.Generic;
using System.Text;

namespace SoftwareShared.Notifications;

public record NotificationRequest
{
    public required string Message { get; set; }
}
