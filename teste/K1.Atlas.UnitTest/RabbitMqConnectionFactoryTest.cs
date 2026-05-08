using K1.Atlas.PubSub.Rabbit;
using K1.Atlas.PubSub.RabbitMQ;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;

namespace K1.Atlas.UnitTest
{
    public class IRabbitMqConnectionFactoryTest
    {
        [Fact]
        public async Task CreateConnectionAsync_ReturnsConnection()
        {
            var optionsMock = new Mock<IOptions<ConfigRabbitMQ>>();
            optionsMock.Setup(o => o.Value).Returns(It.IsAny<ConfigRabbitMQ>());

            var connectionMock = new Mock<IConnection>();
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            connectionFactoryMock
                .Setup(f => f.CreateConnectionAsync(CancellationToken.None))
                .ReturnsAsync(connectionMock.Object);

            var factory = new TestRabbitMqConnectionFactory(optionsMock.Object, connectionFactoryMock.Object);

            var connection = await factory.CreateConnectionAsync();

            Assert.NotNull(connection);
            Assert.Equal(connectionMock.Object, connection);
        }

        private class TestRabbitMqConnectionFactory(IOptions<ConfigRabbitMQ> config, IConnectionFactory connectionFactory) : RabbitMqConnectionFactory(config)
        {
            public new async Task<IConnection> CreateConnectionAsync()
            {
                return await connectionFactory.CreateConnectionAsync();
            }
        }
    }
}