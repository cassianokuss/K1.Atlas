using System.Diagnostics;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.PubSub.Rabbit;
using K1.Atlas.PubSub.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;

namespace K1.Atlas.UnitTest;

public class MessageProducerTest
{
    private readonly Mock<IRabbitMqConnectionFactory> _connectionFactoryMock;
    private readonly Mock<ISerializationManager> _serializationMock;
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IChannel> _channelMock;
    private readonly ActivitySource _activitySource;
    private readonly Mock<ILogger<MessageProducer>> _logger;
    private readonly Mock<IOptions<ConfigRabbitMQ>> _options;


    public MessageProducerTest()
    {
        _connectionFactoryMock = new Mock<IRabbitMqConnectionFactory>();
        _serializationMock = new Mock<ISerializationManager>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IChannel>();
        _activitySource = new ActivitySource("TestSource");
        _logger = new Mock<ILogger<MessageProducer>>();
        _options = new Mock<IOptions<ConfigRabbitMQ>>();

        _connectionFactoryMock
            .Setup(f => f.CreateConnectionAsync())
            .ReturnsAsync(_connectionMock.Object);

        _connectionMock
            .Setup(c => c.IsOpen)
            .Returns(true);

        _connectionMock
            .Setup(c => c.CreateChannelAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_channelMock.Object);

        _options.Setup(o => o.Value)
            .Returns(new ConfigRabbitMQ
            {
                DefaultExchange = "default-exchange",
                PrefetchCount = 10
            });
    }

    [Fact]
    public async Task Publish_CallsBasicPublishAsync_WithCorrectParameters()
    {
        var testObj = new { Name = "Test" };
        var options = new PublishOptions
        {
            Exchange = "test-exchange",
            RoutingKey = "test-key",
            MimeType = "application/json",
            Headers = new Dictionary<string, object>()
        };

        var serialized = new byte[] { 1, 2, 3 };
        _serializationMock
            .Setup(s => s.DefaultMimeType)
            .Returns("application/json");

        _serializationMock
            .Setup(s => s.Serialize(testObj, "application/json"))
            .Returns(serialized);

        var producer = new MessageProducer(_connectionFactoryMock.Object, _serializationMock.Object, _activitySource, _logger.Object, _options.Object);

        await producer.Publish(testObj, options);

        _channelMock.Verify(c => c.BasicPublishAsync(
            options.Exchange,
            options.RoutingKey,
            false,
            It.IsAny<BasicProperties>(), // ou IAmqpHeader dependendo do tipo real
            serialized,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishBatch_CallsBasicPublishAsync_ForEachMessage()
    {
        var testObjs = new[] { "msg1", "msg2" };
        var options = new PublishOptions
        {
            Exchange = "batch-exchange",
            RoutingKey = "batch-key",
            MimeType = "application/json",
            Headers = new Dictionary<string, object>()
        };

        var serializedBatch = new List<byte[]>
            {
                new byte[] { 1, 2 },
                new byte[] { 3, 4 }
            };

        _serializationMock
            .Setup(s => s.DefaultMimeType)
            .Returns("application/json");
        _serializationMock
            .Setup(s => s.SerializeBatch(testObjs, "application/json"))
            .Returns(serializedBatch);

        _channelMock
            .Setup(c => c.BasicPublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask());

        var producer = new MessageProducer(_connectionFactoryMock.Object, _serializationMock.Object, _activitySource, _logger.Object, _options.Object);

        await producer.PublishBatch(testObjs, options);

        _channelMock.Verify(c => c.BasicPublishAsync(
            options.Exchange,
            options.RoutingKey,
            false,
            It.IsAny<BasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(serializedBatch.Count));
    }

    [Fact]
    public async Task Dispose_DisposesConnectionAndChannel()
    {
        var producer = new MessageProducer(_connectionFactoryMock.Object, _serializationMock.Object, _activitySource, _logger.Object, _options.Object);

        // Set private fields using reflection for testing Dispose
        typeof(MessageProducer)
            .GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(producer, _connectionMock.Object);

        typeof(MessageProducer)
            .GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(producer, _channelMock.Object);

        _channelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _connectionMock.Setup(c => c.Dispose());

        await producer.DisposeAsync();

        _connectionMock.Verify(c => c.Dispose(), Times.Once);
        _channelMock.Verify(c => c.DisposeAsync(), Times.Once);
    }
}
