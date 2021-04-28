using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Abstractions.Subscribers;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Subscriber.Models
{
    internal class SubscriberMessage : ISubscriberMessage
    {
        private readonly ReadOnlyMemory<byte> _body;
        private readonly IModel _channel;
        private readonly ulong _deliveryTag;
        private readonly IBasicProperties _properties;
        private readonly ISubscriberSerializer _serializer;
        private bool _isAck;

        public SubscriberMessage(IModel channel, ISubscriberSerializer serializer, ulong deliveryTag, IRouting routing,
            IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            Routing = routing;
            _channel = channel;
            _channel.ConfirmSelect();
            _serializer = serializer;
            _deliveryTag = deliveryTag;
            _body = body;
            _properties = properties;
        }

        public string CorrelationId => _properties.CorrelationId;
        public IDictionary<string, object> Headers => _properties.Headers;
        public IRouting Routing { get; }

        public T GetValue<T>()
        {
            return _serializer.Deserialize<T>(_body);
        }

        public async Task AckAsync(CancellationToken cancellationToken)
        {
            if (_isAck) return;
            _channel.BasicAck(_deliveryTag, false);
            await WaitForConfirmsAsync(cancellationToken);
            _isAck = true;
        }

        public async Task NotAckAsync(CancellationToken cancellationToken)
        {
            if (_isAck) return;
            _channel.BasicNack(_deliveryTag, false, false);
            await WaitForConfirmsAsync(cancellationToken);
            _isAck = true;
        }

        private async Task WaitForConfirmsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested &&
                   !_channel.WaitForConfirms(TimeSpan.FromMilliseconds(100))) await Task.Delay(100, cancellationToken);
        }
    }
}