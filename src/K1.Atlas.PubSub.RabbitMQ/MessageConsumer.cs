using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.Rabbit;
using K1.Atlas.PubSub.Rabbit.Exceptions;
using K1.Atlas.PubSub.RabbitMQ.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace K1.Atlas.PubSub.RabbitMQ;

public sealed class MessageConsumer : MessageAgent<MessageConsumer>, IMessageConsumer, IAsyncDisposable
{
    private readonly ISerializationManager serialization;

    public MessageConsumer(
        IRabbitMqConnectionFactory connectionFactory,
        ISerializationManager serialization,
        ILogger<MessageConsumer> logger,
        IOptions<ConfigRabbitMQ> config
    ) : base(connectionFactory, logger, config)
    {
        this.serialization = serialization;
    }
    
    public async ValueTask DisposeAsync()
    {
        _connection?.Dispose();
        if (_channel != null)
            await _channel.DisposeAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    public async Task<ISubscription> SubscribeAsync<T>(Func<T, IMessageContext, Task> callback, SubscriptionOptions? options = null)
    {
        await EnsureConnectionAsync();
        
        var autoAck = options?.AutoAck ?? true;
        var exchange = ValidateAndGetExchange(options?.Exchange);

        await EnsureExchangeCreatedAsync(exchange, options.ExchangeType());
        var queueName = await EnsureQueueCreatedAsync(exchange, options?.Queue, options?.RoutingKeys);

        var consumer = CreateConsumer(callback, autoAck);
        var consumerTag = await _channel!.BasicConsumeAsync(queue: queueName, autoAck: autoAck, consumer: consumer);

        return new SubscriptionImpl(_channel!, consumerTag);
    }

    private string ValidateAndGetExchange(string? exchange)
    {
        var resolvedExchange = exchange ?? _config.DefaultExchange;
        
        if (resolvedExchange is null)
            throw new InvalidExchangeException();

        return resolvedExchange;
    }

    private AsyncEventingBasicConsumer CreateConsumer<T>(Func<T, IMessageContext, Task> callback, bool autoAck)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += (_, eventArgs) => HandleMessageReceivedAsync(callback, eventArgs, autoAck);
        return consumer;
    }

    private async Task HandleMessageReceivedAsync<T>(
        Func<T, IMessageContext, Task> callback,
        BasicDeliverEventArgs eventArgs,
        bool autoAck)
    {
        var context = new MessageContext(eventArgs, autoAck, _channel!);
        
        try
        {
            var obj = serialization.Deserialize<T>(eventArgs.Body.ToArray(), eventArgs.BasicProperties.ContentType);
            await callback(obj, context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao desserializar mensagem");
        }
    }

    private async Task EnsureExchangeCreatedAsync(string name, string type)
    {
        await _channel!.ExchangeDeclareAsync(name, type, durable: true, autoDelete: false);
    }

    private async Task<string> EnsureQueueCreatedAsync(string exchange, string? queue, IEnumerable<string>? routingKeys)
    {
        var queueName = await DeclareQueueAsync(queue);
        await BindQueueToExchangeAsync(queueName, exchange, routingKeys);
        return queueName;
    }

    private async Task<string> DeclareQueueAsync(string? queue)
    {
        var args = CreateQueueArguments();
        var isTemporaryQueue = string.IsNullOrEmpty(queue);

        var declaredQueue = await _channel!.QueueDeclareAsync(
            queue: queue ?? string.Empty,
            autoDelete: !_config.DurableQueues || isTemporaryQueue,
            durable: _config.DurableQueues,
            exclusive: false,
            arguments: args);

        return declaredQueue.QueueName;
    }

    private Dictionary<string, object?>? CreateQueueArguments()
    {
        return _config.LazyQueues
            ? new Dictionary<string, object?> { { "x-queue-mode", "lazy" } }
            : null;
    }

    private async Task BindQueueToExchangeAsync(string queueName, string exchange, IEnumerable<string>? routingKeys)
    {
        var routingKeysList = GetRoutingKeysList(routingKeys);

        foreach (var routingKey in routingKeysList)
        {
            await _channel!.QueueBindAsync(queueName, exchange, routingKey);
        }
    }

    private static List<string> GetRoutingKeysList(IEnumerable<string>? routingKeys)
    {
        var routingKeysList = routingKeys?.ToList() ?? [];
        
        if (!routingKeysList.Any())
            routingKeysList.Add(string.Empty);

        return routingKeysList;
    }
}
