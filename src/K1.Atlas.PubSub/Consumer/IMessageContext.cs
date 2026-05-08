namespace K1.Atlas.PubSub.Consumer
{
    public interface IMessageContext
    {
        IDictionary<string, object> Headers { get; }

        ValueTask AckAsync(CancellationToken cancellationToken = default);
        ValueTask NackAsync(bool requeue = false, CancellationToken cancellationToken = default);
        ValueTask RejectAsync(bool requeue = false, CancellationToken cancellationToken = default);
    }
}
