using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions;
using EasyRabbitMqClient.Abstractions.Builders;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core.Builders
{
    public class MessagePublisherBuilder<T> : IMessagePublisherBuilder<T>
    {
        private readonly IAsyncConnectionFactory _connectionFactory;
        private IExchange _exchange;
        private string _routingKey;
        private IPublisherSerializer _serializer;
        private ICollection<T> _buffer;

        public MessagePublisherBuilder(IAsyncConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IExchangePublisherBuilder<T> WithExchange()
        {
            return new ExchangePublisherBuilder<T>(this);
        }

        public IMessagePublisherBuilder<T> WithExchange(IExchange exchange)
        {
            _exchange = exchange;
            return this;
        }

        public IMessagePublisherBuilder<T> WithRoutingKey(string routingKey)
        {
            _routingKey = routingKey;
            return this;
        }

        public IMessagePublisherBuilder<T> WithSerializer(IPublisherSerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public IBufferPublisherBuilder<T> WithBuffer()
        {
            return new BufferPublisherBuilder<T>(this);
        }
        
        public IMessagePublisherBuilder<T> WithBuffer(ICollection<T> buffer)
        {
            _buffer = buffer;
            return this;
        }

        public IMessagePublisher<T> Build()
        {
            return new MessagePublisher<T>(_connectionFactory.CreateConnection(), _serializer, _buffer, _exchange, _routingKey);
        }
    }
}