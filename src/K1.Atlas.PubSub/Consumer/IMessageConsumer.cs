namespace K1.Atlas.PubSub.Consumer
{
    public interface IMessageConsumer : IDisposable
    {
        Task<ISubscription> SubscribeAsync<T>(Func<T, IMessageContext, Task> callback, SubscriptionOptions? options = null);
    }
}
