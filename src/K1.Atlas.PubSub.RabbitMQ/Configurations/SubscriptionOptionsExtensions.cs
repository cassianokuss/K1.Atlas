
using RabbitClient = RabbitMQ.Client;
using K1.Atlas.PubSub.Consumer;

namespace K1.Atlas.PubSub.RabbitMQ.Configurations;

public static class SubscriptionOptionsExtensions
{
    public static string ExchangeType(this SubscriptionOptions? options)
    {
        if (options is { RoutingKeys: not null } && options.RoutingKeys.Any())
            return RabbitClient.ExchangeType.Topic;

        return RabbitClient.ExchangeType.Fanout;
    }
}
