using System.Diagnostics;
using K1.Atlas.PubSub.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace K1.Atlas.PubSub.HostedServices
{
    public class AsyncConsumerService<TObj, TService>(
        IMessageConsumer consumer,
        IServiceScopeFactory scopeFactory,
        Action<IAsyncConsumerOptionsBuilder<TObj>>? builder,
        ActivitySource activitySource) : IHostedService, IDisposable
        where TService : IBackgroundConsumer<TObj>
    {
        private readonly CancellationTokenSource _stoppingCts = new();
        private bool _disposed = false;
        private ISubscription? _subscription;
        private AsyncConsumerOptions<TObj>? _options;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stoppingCts.Dispose();
                }
                _disposed = true;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a scope to resolve options
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var optionsBuilder = new AsyncConsumerOptionsBuilder<TObj>(provider);
            builder?.Invoke(optionsBuilder);
            _options = optionsBuilder.Options;

            _subscription = await consumer.SubscribeAsync<TObj>(async (obj, context) =>
            {
                // Create a new scope for each message
                using var messageScope = scopeFactory.CreateScope();
                var service = messageScope.ServiceProvider.GetRequiredService<TService>();
                var manager = new PipelineManager<TObj>(service, _options.Pipelines, _stoppingCts.Token, activitySource);
                await manager.Run(obj, context);
            }, new SubscriptionOptions
            {
                AutoAck = _options.AutoAck,
                Exchange = _options.Exchange,
                Queue = _options.QueueName,
                RoutingKeys = _options.RoutingKeys
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _stoppingCts.CancelAsync();
            if (_subscription != null)
            {
                await _subscription.DisposeAsync();
            }
        }
    }
}