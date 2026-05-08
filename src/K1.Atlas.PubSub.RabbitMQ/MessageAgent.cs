using K1.Atlas.PubSub.Rabbit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace K1.Atlas.PubSub.RabbitMQ;

public class MessageAgent<T>(
    IRabbitMqConnectionFactory connectionFactory,
    ILogger<T> logger,
    IOptions<ConfigRabbitMQ> config
)
{
    protected IConnection? _connection;
    protected IChannel? _channel;
    protected readonly ConfigRabbitMQ _config = config.Value;
    protected readonly ILogger<T> logger = logger;

    protected async Task EnsureConnectionAsync()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = await connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            _channel.CallbackExceptionAsync += (_, e) =>
            {
                logger.LogError(e.Exception, "Erro no processamento da mensagem");
                return Task.CompletedTask;
            };

            if (_config.PrefetchCount.HasValue)
                await _channel.BasicQosAsync(0, _config.PrefetchCount.Value, global: false);
        }
    }
}
