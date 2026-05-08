using K1.Atlas.PubSub.Producer;

namespace K1.Atlas.UnitTest.PubSub;

public class PublishOptionsTest
{
    [Fact]
    public void Constructor_Should_Initialize_Empty_Options()
    {
        // Arrange & Act
        var options = new PublishOptions();

        // Assert
        Assert.Null(options.Exchange);
        Assert.Null(options.Headers);
        Assert.Null(options.MimeType);
        Assert.Null(options.RoutingKey);
    }

    [Fact]
    public void ToExchange_Should_Set_Exchange()
    {
        // Arrange
        var options = new PublishOptions();

        // Act
        var result = options.ToExchange("test-exchange");

        // Assert
        Assert.Equal("test-exchange", options.Exchange);
        Assert.Same(options, result);
    }

    [Fact]
    public void ToRoutingKey_Should_Set_RoutingKey()
    {
        // Arrange
        var options = new PublishOptions();

        // Act
        var result = options.ToRoutingKey("test.routing.key");

        // Assert
        Assert.Equal("test.routing.key", options.RoutingKey);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithHeaders_Should_Set_Headers()
    {
        // Arrange
        var options = new PublishOptions();
        var headers = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", 123 }
        };

        // Act
        var result = options.WithHeaders(headers);

        // Assert
        Assert.Same(headers, options.Headers);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithMimeType_Should_Set_MimeType()
    {
        // Arrange
        var options = new PublishOptions();

        // Act
        var result = options.WithMimeType("application/json");

        // Assert
        Assert.Equal("application/json", options.MimeType);
        Assert.Same(options, result);
    }

    [Fact]
    public void RoutingTo_Should_Create_Options_With_RoutingKey()
    {
        // Arrange & Act
        var options = PublishOptions.RoutingTo("test.key");

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test.key", options.RoutingKey);
    }

    [Fact]
    public void Fluent_Interface_Should_Work()
    {
        // Arrange & Act
        var options = new PublishOptions()
            .ToExchange("my-exchange")
            .ToRoutingKey("my.routing.key")
            .WithMimeType("application/json")
            .WithHeaders(new Dictionary<string, object> { { "correlation-id", "12345" } });

        // Assert
        Assert.Equal("my-exchange", options.Exchange);
        Assert.Equal("my.routing.key", options.RoutingKey);
        Assert.Equal("application/json", options.MimeType);
        Assert.NotNull(options.Headers);
        Assert.Single(options.Headers);
    }

    [Fact]
    public void Static_Factory_And_Fluent_Should_Work_Together()
    {
        // Arrange & Act
        var options = PublishOptions.RoutingTo("events.created")
            .ToExchange("domain-events")
            .WithMimeType("application/msgpack");

        // Assert
        Assert.Equal("events.created", options.RoutingKey);
        Assert.Equal("domain-events", options.Exchange);
        Assert.Equal("application/msgpack", options.MimeType);
    }
}
