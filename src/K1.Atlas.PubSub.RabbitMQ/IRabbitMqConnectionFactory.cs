using K1.Atlas.PubSub.Rabbit;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace K1.Atlas.PubSub.RabbitMQ;

public interface IRabbitMqConnectionFactory
{
    Task<IConnection> CreateConnectionAsync();
}


public class RabbitMqConnectionFactory(IOptions<ConfigRabbitMQ> config) : IRabbitMqConnectionFactory
{
    private readonly ConfigRabbitMQ _config = config.Value;

    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.Host,
            Port = _config.Port ?? 5672,
            UserName = string.IsNullOrWhiteSpace(_config.User) ? "guest" : _config.User,
            Password = string.IsNullOrWhiteSpace(_config.Password) ? "guest" : _config.Password,
            VirtualHost = string.IsNullOrWhiteSpace(_config.VirtualHost) ? "/" : _config.VirtualHost
        };

        return await factory.CreateConnectionAsync();
    }
}
