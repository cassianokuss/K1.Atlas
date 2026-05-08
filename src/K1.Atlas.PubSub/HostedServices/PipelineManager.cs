using System.Diagnostics;
using System.Text;
using K1.Atlas.PubSub.Consumer;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace K1.Atlas.PubSub.HostedServices
{
    public class PipelineManager<TObj>
    {
        private int _index;
        private TObj _obj = default!;
        private IMessageContext _context = default!;

        private readonly Func<Task> _next;
        private readonly Func<TObj, IMessageContext, CancellationToken, Func<Task>, Task>[] _pipelines;
        private readonly Func<TObj, IMessageContext, CancellationToken, Task> _service;
        private readonly CancellationToken _cancellationToken;
        private readonly ActivitySource _activitySource;

        public PipelineManager(
            IBackgroundConsumer<TObj> service,
            IEnumerable<IConsumerPipeline<TObj>> pipelines,
            CancellationToken cancellationToken
,
            ActivitySource activitySource)
        {
            _service = service.ConsumeAsync;
            _pipelines = pipelines.Select(p => (Func<TObj, IMessageContext, CancellationToken, Func<Task>, Task>)p.Handle).ToArray();
            _cancellationToken = cancellationToken;
            _activitySource = activitySource;

            _next = () =>
            {
                if (_index == _pipelines.Length)
                {
                    if (_context.Headers == null || _context.Headers.Count == 0)
                        return _service(_obj, _context, _cancellationToken);

                    var propagator = Propagators.DefaultTextMapPropagator;
                    var parentContext = propagator.Extract(default, _context.Headers, ExtractTraceContextFromBasicProperties);
                    Baggage.Current = parentContext.Baggage;

                    using var activity = _activitySource?.StartActivity(_obj.GetType().Name, ActivityKind.Consumer, parentContext.ActivityContext);
                    return _service(_obj, _context, _cancellationToken);
                }

                return _pipelines[_index++](_obj, _context, _cancellationToken, _next!);
            };
        }

        public Task Run(TObj obj, IMessageContext context)
        {
            _index = 0;
            _obj = obj;
            _context = context;

            return _next();
        }

        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IDictionary<string, object>? props, string key)
        {
            if (props != null && props.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];

                if (bytes != null)
                    return new[] { Encoding.UTF8.GetString(bytes) };
            }

            return Enumerable.Empty<string>();
        }
    }
}
