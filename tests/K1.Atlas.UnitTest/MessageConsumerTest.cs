using K1.Atlas.PubSub.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using K1.Atlas.PubSub.Rabbit;
using K1.Atlas.PubSub.Rabbit.Exceptions;

namespace K1.Atlas.UnitTest
{
    public class MessageConsumerTest
    {
        [Fact]
        public async Task SubscribeAsync_Should_Throw_When_Exchange_Is_Null()
        {
            var connectionFactory = new Mock<IRabbitMqConnectionFactory>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IChannel>();
            connectionFactory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);
            connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(channel.Object);

            var serialization = new Mock<ISerializationManager>();
            var logger = new Mock<ILogger<MessageConsumer>>();
            var config = Options.Create(new ConfigRabbitMQ { DefaultExchange = null });

            var consumer = new MessageConsumer(connectionFactory.Object, serialization.Object, logger.Object, config);

            await Assert.ThrowsAsync<InvalidExchangeException>(async () =>
            {
                await consumer.SubscribeAsync<string>((msg, ctx) => Task.CompletedTask, null);
            });
        }

        [Fact]
        public async Task DisposeAsync_Should_Dispose_Connection_And_Channel()
        {
            var connection = new Mock<IConnection>();
            var channel = new Mock<IChannel>();
            var connectionFactory = new Mock<IRabbitMqConnectionFactory>();
            var serialization = new Mock<ISerializationManager>();
            var logger = new Mock<ILogger<MessageConsumer>>();
            var config = Options.Create(new ConfigRabbitMQ { DefaultExchange = "test" });

            connectionFactory.Setup(f => f.CreateConnectionAsync()).ReturnsAsync(connection.Object);
            connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(channel.Object);

            var consumer = new MessageConsumer(connectionFactory.Object, serialization.Object, logger.Object, config);
            // Simular conexão e canal privados
            typeof(MessageConsumer).GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(consumer, connection.Object);
            typeof(MessageConsumer).GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(consumer, channel.Object);

            await consumer.DisposeAsync();

            connection.Verify(c => c.Dispose(), Times.Once);
            channel.Verify(c => c.DisposeAsync(), Times.Once);
        }
    }
}
