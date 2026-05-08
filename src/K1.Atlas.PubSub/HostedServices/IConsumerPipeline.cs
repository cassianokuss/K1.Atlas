using K1.Atlas.PubSub.Consumer;

namespace K1.Atlas.PubSub.HostedServices
{
    public interface IConsumerPipeline<T>
    {
        Task Handle(T obj, IMessageContext context, CancellationToken cancellationToken, Func<Task> next);
    }
}
