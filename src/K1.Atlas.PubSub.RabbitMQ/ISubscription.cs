using K1.Atlas.PubSub.Consumer;
using RabbitMQ.Client;

namespace K1.Atlas.PubSub.Rabbit
{
    internal class SubscriptionImpl : ISubscription
    {
        private readonly string _consumerTag;
        private readonly IChannel _channel;
        private bool _disposed = false;

        public SubscriptionImpl(IChannel channel, string consumerTag)
        {
            _consumerTag = consumerTag;
            _channel = channel;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected async Task DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await _channel.BasicCancelAsync(_consumerTag);
                    await _channel.DisposeAsync();
                }

                _disposed = true;
            }
        }
    }
}
