using System.Diagnostics;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.PubSub.Rabbit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace K1.Atlas.PubSub.RabbitMQ;

public sealed class MessageProducer(
    IRabbitMqConnectionFactory connectionFactory,
    ISerializationManager serialization,
    ActivitySource activitySource,
    ILogger<MessageProducer> logger,
    IOptions<ConfigRabbitMQ> options)
    : MessageAgent<MessageProducer>(connectionFactory, logger, options), IMessageProducer, IAsyncDisposable
{

    public async ValueTask DisposeAsync()
    {
        _connection?.Dispose();
        await _channel!.DisposeAsync();
    }

    public async Task Publish<T>(T obj, PublishOptions options) where T : class
    {
        await EnsureConnectionAsync();

        using var activity = activitySource.StartActivity($"RMQ.Publish - {options.Exchange}/{options.RoutingKey}", ActivityKind.Producer);
        ConfigureActivityTags(activity, options);

        try
        {
            InjectTelemetryContext(activity, options);
            var properties = CreateBasicProperties(options);
            var body = serialization.Serialize(obj, properties.ContentType);
            var exchange = ValidateAndGetExchange(options.Exchange);

            await _channel!.BasicPublishAsync(
                exchange: exchange,
                routingKey: options.RoutingKey ?? string.Empty,
                mandatory: false,
                basicProperties: properties,
                body: body);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    public async Task PublishBatch<T>(IEnumerable<T> obj, PublishOptions options)
    {
        await EnsureConnectionAsync();

        var properties = CreateBasicProperties(options);
        var exchange = ValidateAndGetExchange(options.Exchange);
        var messages = serialization.SerializeBatch(obj, options.MimeType ?? serialization.DefaultMimeType);

        var publishTasks = messages.Select(message =>
            _channel!.BasicPublishAsync(
                exchange: exchange,
                routingKey: options.RoutingKey ?? string.Empty,
                mandatory: false,
                basicProperties: properties,
                body: new ReadOnlyMemory<byte>(message)));

        await Task.WhenAll(publishTasks.Select(vt => vt.AsTask()));
    }

    private static void ConfigureActivityTags(Activity? activity, PublishOptions options)
    {
        if (activity == null)
            return;

        activity.SetTag("messaging.system", "rabbitmq");
        activity.SetTag("messaging.exchange", options.Exchange);
        activity.SetTag("messaging.routing_key", options.RoutingKey);
    }

    private static void InjectTelemetryContext(Activity? activity, PublishOptions options)
    {
        if (activity == null)
            return;

        options.Headers ??= new Dictionary<string, object>();
        Propagators.DefaultTextMapPropagator.Inject(
            new PropagationContext(activity.Context, Baggage.Current),
            options.Headers,
            InjectContextIntoHeader);
    }

    private BasicProperties CreateBasicProperties(PublishOptions options)
    {
        return new BasicProperties
        {
            Persistent = true,
            ContentType = options.MimeType ?? serialization.DefaultMimeType,
            Headers = options.Headers?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value)
        };
    }

    private string ValidateAndGetExchange(string? exchange)
    {
        var resolvedExchange = exchange ?? _config.DefaultExchange;

        if (resolvedExchange == null)
            throw new InvalidOperationException("Exchange must be specified either in options or configuration.");

        return resolvedExchange;
    }

    private static void InjectContextIntoHeader(IDictionary<string, object> props, string key, string value)
    {
        props[key] = value;
    }
}
