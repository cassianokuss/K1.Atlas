using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.Rabbit.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace K1.Atlas.PubSub.RabbitMQ
{

    public class MessageContext : IMessageContext
    {
        private readonly BasicDeliverEventArgs _args;
        private readonly bool _autoAck;
        private readonly IChannel _channel;

        private bool _ackFired;

        public IDictionary<string, object> Headers =>
            (_args.BasicProperties.Headers as IDictionary<string, object>) ??
            new Dictionary<string, object>();

        internal MessageContext(BasicDeliverEventArgs args, bool autoAck, IChannel channel)
        {
            _args = args;
            _autoAck = autoAck;
            _channel = channel;
        }

        public ValueTask AckAsync(CancellationToken cancellationToken = default)
        {
            EnsureCanAck();
            return _channel.BasicAckAsync(_args.DeliveryTag, false, cancellationToken);
        }

        public ValueTask NackAsync(bool requeue = false, CancellationToken cancellationToken = default)
        {
            EnsureCanAck();
            return _channel.BasicNackAsync(_args.DeliveryTag, false, requeue, cancellationToken);
        }

        public ValueTask RejectAsync(bool requeue = false, CancellationToken cancellationToken = default)
        {
            EnsureCanAck();
            return _channel.BasicRejectAsync(_args.DeliveryTag, requeue, cancellationToken);
        }

        private void EnsureCanAck()
        {
            if (_autoAck)
            {
                throw new MessageAckAlreadyFiredException("Automatic ack is enabled for this consumer. Cannot ack twice.");
            }

            if (_ackFired)
            {
                throw new MessageAckAlreadyFiredException();
            }

            _ackFired = true;
        }
    }
}
