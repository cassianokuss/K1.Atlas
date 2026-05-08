namespace K1.Atlas.PubSub.Producer;

public interface IMessageProducer
{
    Task Publish<T>(T obj, PublishOptions options) where T : class;
    Task PublishBatch<T>(IEnumerable<T> obj, PublishOptions options);
}
