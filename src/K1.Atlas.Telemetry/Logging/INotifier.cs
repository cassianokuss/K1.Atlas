using Microsoft.Extensions.Logging;

namespace K1.Atlas.Telemetry.Logging;

public interface INotifier
{
    bool HasFailNotification { get; }
    bool HasNotification { get; }
    IEnumerable<Notification> GetNotifications();
    void NotifyWarning(string message, params object?[] args);
    void NotifyError(string message, params object?[] args);
    void NotifyInformation(string message, params object?[] args);
}

public class Notifier : INotifier
{
    private readonly IList<Notification> _notification;
    private readonly ILogger<Notifier> _logger;

    public Notifier(ILogger<Notifier> logger)
    {
        _notification = new List<Notification>();
        _logger = logger;
    }

    public IEnumerable<Notification> GetNotifications()
    {
        return _notification;
    }

    public void NotifyWarning(string message, params object?[] args)
    {
        var notification = new Notification(NotificationType.Warning, message, args);
        _notification.Add(notification);
        LogNotification(notification);
    }

    public void NotifyError(string message, params object?[] args)
    {
        var notification = new Notification(NotificationType.Error, message, args);
        _notification.Add(new Notification(NotificationType.Error, message, args));
        LogNotification(notification);
    }

    public void NotifyInformation(string message, params object?[] args)
    {
        var notification = new Notification(NotificationType.Information, message, args);
        _notification.Add(new Notification(NotificationType.Information, message, args));
        LogNotification(notification);
    }

    private void LogNotification(Notification notification)
    {
        var message = notification.Message;

        switch (notification.NotificationType)
        {
            case NotificationType.Information:
                _logger.LogInformation(message, notification.Args);
                break;
            case NotificationType.Warning:
                _logger.LogWarning(message, notification.Args);
                break;
            case NotificationType.Error:
                _logger.LogError(message, notification.Args);
                break;
        }
    }

    public bool HasFailNotification => _notification.Any(e => e.NotificationType == NotificationType.Warning || e.NotificationType == NotificationType.Error);
    public bool HasNotification => _notification.Any();
}

public struct Notification
{
    public NotificationType NotificationType { get; }
    public string Message { get; }
    public object?[] Args { get; }

    public Notification(NotificationType notificationType, string message, params object?[] args)
    {
        NotificationType = notificationType;
        Message = message;
        Args = args;
    }
}

public enum NotificationType
{
    Information,
    Warning,
    Error
}
