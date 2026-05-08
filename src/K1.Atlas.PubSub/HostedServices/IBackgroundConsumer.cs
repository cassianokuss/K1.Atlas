using K1.Atlas.PubSub.Consumer;

namespace K1.Atlas.PubSub.HostedServices
{
    public interface IBackgroundConsumer<in T>
    {
        Task ConsumeAsync(T obj, IMessageContext context, CancellationToken cancellationToken);
    }
}
