using K1.Atlas.Telemetry.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace K1.Atlas.UnitTest.Telemetry;

public class NotifierTest
{
    [Fact]
    public void Constructor_Should_Initialize_Empty_Notifications()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();

        // Act
        var notifier = new Notifier(logger.Object);

        // Assert
        Assert.False(notifier.HasNotification);
        Assert.False(notifier.HasFailNotification);
        Assert.Empty(notifier.GetNotifications());
    }

    [Fact]
    public void NotifyInformation_Should_Add_Information_Notification()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Test information message");

        // Assert
        Assert.True(notifier.HasNotification);
        Assert.False(notifier.HasFailNotification);
        var notifications = notifier.GetNotifications().ToList();
        Assert.Single(notifications);
        Assert.Equal(NotificationType.Information, notifications[0].NotificationType);
        Assert.Equal("Test information message", notifications[0].Message);
    }

    [Fact]
    public void NotifyWarning_Should_Add_Warning_Notification()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyWarning("Test warning message");

        // Assert
        Assert.True(notifier.HasNotification);
        Assert.True(notifier.HasFailNotification);
        var notifications = notifier.GetNotifications().ToList();
        Assert.Single(notifications);
        Assert.Equal(NotificationType.Warning, notifications[0].NotificationType);
        Assert.Equal("Test warning message", notifications[0].Message);
    }

    [Fact]
    public void NotifyError_Should_Add_Error_Notification()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyError("Test error message");

        // Assert
        Assert.True(notifier.HasNotification);
        Assert.True(notifier.HasFailNotification);
        var notifications = notifier.GetNotifications().ToList();
        Assert.Single(notifications);
        Assert.Equal(NotificationType.Error, notifications[0].NotificationType);
        Assert.Equal("Test error message", notifications[0].Message);
    }

    [Fact]
    public void NotifyInformation_Should_Log_Information()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Test message with {0}", "arg");

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void NotifyWarning_Should_Log_Warning()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyWarning("Test warning");

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void NotifyError_Should_Log_Error()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyError("Test error");

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void Multiple_Notifications_Should_Be_Tracked()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Info 1");
        notifier.NotifyWarning("Warning 1");
        notifier.NotifyError("Error 1");
        notifier.NotifyInformation("Info 2");

        // Assert
        Assert.True(notifier.HasNotification);
        Assert.True(notifier.HasFailNotification);
        var notifications = notifier.GetNotifications().ToList();
        Assert.Equal(4, notifications.Count);
    }

    [Fact]
    public void HasFailNotification_Should_Be_True_With_Warning()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Info");
        notifier.NotifyWarning("Warning");

        // Assert
        Assert.True(notifier.HasFailNotification);
    }

    [Fact]
    public void HasFailNotification_Should_Be_True_With_Error()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Info");
        notifier.NotifyError("Error");

        // Assert
        Assert.True(notifier.HasFailNotification);
    }

    [Fact]
    public void HasFailNotification_Should_Be_False_With_Only_Information()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Info 1");
        notifier.NotifyInformation("Info 2");

        // Assert
        Assert.False(notifier.HasFailNotification);
    }

    [Fact]
    public void Notification_Should_Store_Args()
    {
        // Arrange
        var logger = new Mock<ILogger<Notifier>>();
        var notifier = new Notifier(logger.Object);

        // Act
        notifier.NotifyInformation("Test {0} {1}", "arg1", 123);

        // Assert
        var notification = notifier.GetNotifications().First();
        Assert.Equal(2, notification.Args.Length);
        Assert.Equal("arg1", notification.Args[0]);
        Assert.Equal(123, notification.Args[1]);
    }
}

public class NotificationStructTest
{
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        // Arrange & Act
        var notification = new Notification(NotificationType.Warning, "Test message", "arg1", 123);

        // Assert
        Assert.Equal(NotificationType.Warning, notification.NotificationType);
        Assert.Equal("Test message", notification.Message);
        Assert.Equal(2, notification.Args.Length);
    }

    [Fact]
    public void Constructor_Should_Handle_No_Args()
    {
        // Arrange & Act
        var notification = new Notification(NotificationType.Information, "Simple message");

        // Assert
        Assert.Equal(NotificationType.Information, notification.NotificationType);
        Assert.Equal("Simple message", notification.Message);
        Assert.Empty(notification.Args);
    }
}
