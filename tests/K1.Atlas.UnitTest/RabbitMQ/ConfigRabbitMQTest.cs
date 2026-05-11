using K1.Atlas.PubSub.Rabbit;

namespace K1.Atlas.UnitTest.RabbitMQ;

public class ConfigRabbitMQTest
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        // Arrange & Act
        var config = new ConfigRabbitMQ();

        // Assert
        Assert.Null(config.DefaultExchange);
        Assert.False(config.DurableQueues);
        Assert.False(config.LazyQueues);
        Assert.False(config.PersistentDelivery);
        Assert.Null(config.PrefetchCount);
    }

    [Fact]
    public void Host_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.Host = "rabbitmq.example.com";

        // Assert
        Assert.Equal("rabbitmq.example.com", config.Host);
    }

    [Fact]
    public void Port_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.Port = 5672;

        // Assert
        Assert.Equal(5672, config.Port);
    }

    [Fact]
    public void Password_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.Password = "secret123";

        // Assert
        Assert.Equal("secret123", config.Password);
    }

    [Fact]
    public void User_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.User = "admin";

        // Assert
        Assert.Equal("admin", config.User);
    }

    [Fact]
    public void VirtualHost_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.VirtualHost = "/production";

        // Assert
        Assert.Equal("/production", config.VirtualHost);
    }

    [Fact]
    public void DefaultExchange_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.DefaultExchange = "my-exchange";

        // Assert
        Assert.Equal("my-exchange", config.DefaultExchange);
    }

    [Fact]
    public void DurableQueues_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.DurableQueues = true;

        // Assert
        Assert.True(config.DurableQueues);
    }

    [Fact]
    public void LazyQueues_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.LazyQueues = true;

        // Assert
        Assert.True(config.LazyQueues);
    }

    [Fact]
    public void PersistentDelivery_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.PersistentDelivery = true;

        // Assert
        Assert.True(config.PersistentDelivery);
    }

    [Fact]
    public void PrefetchCount_Should_Be_Settable()
    {
        // Arrange
        var config = new ConfigRabbitMQ();

        // Act
        config.PrefetchCount = 100;

        // Assert
        Assert.Equal((ushort)100, config.PrefetchCount);
    }

    [Fact]
    public void All_Properties_Should_Be_Settable_Together()
    {
        // Arrange & Act
        var config = new ConfigRabbitMQ
        {
            Host = "localhost",
            Port = 5672,
            User = "guest",
            Password = "guest",
            VirtualHost = "/",
            DefaultExchange = "events",
            DurableQueues = true,
            LazyQueues = true,
            PersistentDelivery = true,
            PrefetchCount = 50
        };

        // Assert
        Assert.Equal("localhost", config.Host);
        Assert.Equal(5672, config.Port);
        Assert.Equal("guest", config.User);
        Assert.Equal("guest", config.Password);
        Assert.Equal("/", config.VirtualHost);
        Assert.Equal("events", config.DefaultExchange);
        Assert.True(config.DurableQueues);
        Assert.True(config.LazyQueues);
        Assert.True(config.PersistentDelivery);
        Assert.Equal((ushort)50, config.PrefetchCount);
    }
}
