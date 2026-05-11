using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace K1.Atlas.UnitTest.Domain;

public class PublisherTest
{
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    [Fact]
    public async Task PublishAsync_Should_Call_All_Registered_Handlers()
    {
        // Arrange
        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        var handler2 = new Mock<INotificationHandler<TestNotification>>();
        var handler3 = new Mock<INotificationHandler<TestNotification>>();

        handler1.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handler2.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handler3.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler1.Object);
        services.AddSingleton(handler2.Object);
        services.AddSingleton(handler3.Object);
        
        var serviceProvider = services.BuildServiceProvider();
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler1.Object, handler2.Object, handler3.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification { Message = "Test", Count = 42 };

        // Act
        await publisher.PublishAsync(notification);

        // Assert
        handler1.Verify(h => h.HandleAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        handler3.Verify(h => h.HandleAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_Should_Handle_No_Handlers()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(Enumerable.Empty<INotificationHandler<TestNotification>>());

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification { Message = "Test" };

        // Act & Assert - Should not throw
        await publisher.PublishAsync(notification);
    }

    [Fact]
    public async Task PublishAsync_Should_Call_Single_Handler()
    {
        // Arrange
        var handler = new Mock<INotificationHandler<TestNotification>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification { Message = "Single Handler Test" };

        // Act
        await publisher.PublishAsync(notification);

        // Assert
        handler.Verify(h => h.HandleAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_Should_Pass_CancellationToken_To_Handlers()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var handler = new Mock<INotificationHandler<TestNotification>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification();

        // Act
        await publisher.PublishAsync(notification, cts.Token);

        // Assert
        handler.Verify(h => h.HandleAsync(notification, cts.Token), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_Should_Execute_Handlers_In_Parallel()
    {
        // Arrange
        var handler1Executed = false;
        var handler2Executed = false;
        var handler3Executed = false;

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        handler1.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                handler1Executed = true;
            });

        var handler2 = new Mock<INotificationHandler<TestNotification>>();
        handler2.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                handler2Executed = true;
            });

        var handler3 = new Mock<INotificationHandler<TestNotification>>();
        handler3.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                handler3Executed = true;
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler1.Object, handler2.Object, handler3.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await publisher.PublishAsync(notification);
        stopwatch.Stop();

        // Assert
        Assert.True(handler1Executed);
        Assert.True(handler2Executed);
        Assert.True(handler3Executed);
    }

    [Fact]
    public async Task PublishAsync_Should_Wait_For_All_Handlers_To_Complete()
    {
        // Arrange
        var completionOrder = new List<int>();
        var lockObj = new object();

        var handler1 = new Mock<INotificationHandler<TestNotification>>();
        handler1.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                lock (lockObj) { completionOrder.Add(1); }
            });

        var handler2 = new Mock<INotificationHandler<TestNotification>>();
        handler2.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                lock (lockObj) { completionOrder.Add(2); }
            });

        var handler3 = new Mock<INotificationHandler<TestNotification>>();
        handler3.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(25);
                lock (lockObj) { completionOrder.Add(3); }
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler1.Object, handler2.Object, handler3.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification();

        // Act
        await publisher.PublishAsync(notification);

        // Assert
        Assert.Equal(3, completionOrder.Count);
        Assert.Contains(1, completionOrder);
        Assert.Contains(2, completionOrder);
        Assert.Contains(3, completionOrder);
    }

    [Fact]
    public async Task PublishAsync_Should_Propagate_Handler_Exception()
    {
        // Arrange
        var handler = new Mock<INotificationHandler<TestNotification>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<INotificationHandler<TestNotification>>)))
            .Returns(new[] { handler.Object });

        var publisher = new Mediator(mockServiceProvider.Object);
        var notification = new TestNotification();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            publisher.PublishAsync(notification));
    }

    [Fact]
    public void TestNotification_Should_Implement_INotification()
    {
        // Arrange & Act
        var notification = new TestNotification();

        // Assert
        Assert.IsAssignableFrom<INotification>(notification);
    }

    [Fact]
    public void INotificationHandler_Should_Have_HandleAsync_Method()
    {
        // Arrange & Act
        var handlerType = typeof(INotificationHandler<TestNotification>);
        var method = handlerType.GetMethod("HandleAsync");

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }
}
