using K1.Atlas.PubSub.Consumer;

namespace K1.Atlas.UnitTest.PubSub;

public class SubscriptionOptionsTest
{
    [Fact]
    public void Constructor_Should_Initialize_Empty_Options()
    {
        // Arrange & Act
        var options = new SubscriptionOptions();

        // Assert
        Assert.Null(options.AutoAck);
        Assert.Null(options.Exchange);
        Assert.Null(options.Queue);
        Assert.Null(options.RoutingKeys);
    }

    [Fact]
    public void AutoAck_Should_Be_Settable()
    {
        // Arrange & Act
        var options = new SubscriptionOptions
        {
            AutoAck = true
        };

        // Assert
        Assert.True(options.AutoAck);
    }

    [Fact]
    public void Exchange_Should_Be_Settable()
    {
        // Arrange & Act
        var options = new SubscriptionOptions
        {
            Exchange = "test-exchange"
        };

        // Assert
        Assert.Equal("test-exchange", options.Exchange);
    }

    [Fact]
    public void Queue_Should_Be_Settable()
    {
        // Arrange & Act
        var options = new SubscriptionOptions
        {
            Queue = "test-queue"
        };

        // Assert
        Assert.Equal("test-queue", options.Queue);
    }

    [Fact]
    public void RoutingKeys_Should_Be_Settable()
    {
        // Arrange
        var routingKeys = new List<string> { "key1", "key2", "key3" };

        // Act
        var options = new SubscriptionOptions
        {
            RoutingKeys = routingKeys
        };

        // Assert
        Assert.Equal(routingKeys, options.RoutingKeys);
    }

    [Fact]
    public void All_Properties_Should_Be_Settable()
    {
        // Arrange
        var routingKeys = new[] { "event.created", "event.updated" };

        // Act
        var options = new SubscriptionOptions
        {
            AutoAck = false,
            Exchange = "events-exchange",
            Queue = "event-processor-queue",
            RoutingKeys = routingKeys
        };

        // Assert
        Assert.False(options.AutoAck);
        Assert.Equal("events-exchange", options.Exchange);
        Assert.Equal("event-processor-queue", options.Queue);
        Assert.Equal(2, options.RoutingKeys!.Count());
    }

    [Fact]
    public void AutoAck_False_Should_Work()
    {
        // Arrange & Act
        var options = new SubscriptionOptions
        {
            AutoAck = false
        };

        // Assert
        Assert.False(options.AutoAck);
    }

    [Fact]
    public void Empty_RoutingKeys_Should_Work()
    {
        // Arrange & Act
        var options = new SubscriptionOptions
        {
            RoutingKeys = Array.Empty<string>()
        };

        // Assert
        Assert.NotNull(options.RoutingKeys);
        Assert.Empty(options.RoutingKeys);
    }
}
