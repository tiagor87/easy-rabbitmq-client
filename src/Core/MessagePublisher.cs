using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core
{
    public class MessagePublisher<T> : IMessagePublisher<T>
    {
        private readonly IConnection _connection;
        private readonly IPublisherSerializer _serializer;
        private readonly ICollection<T> _buffer;
        private readonly IExchange _exchange;
        private readonly string _routingKey;

        public MessagePublisher(
            IConnection connection,
            IPublisherSerializer serializer,
            ICollection<T> buffer,
            IExchange exchange,
            string routingKey)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            _routingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        }

        public Task PublishAsync(T message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}