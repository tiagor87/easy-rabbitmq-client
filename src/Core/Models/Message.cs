using System;
using System.Collections.Generic;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;

namespace EasyRabbitMqClient.Core.Models
{
    public class Message : IMessage
    {
        private readonly object _message;
        private readonly IDictionary<string, object> _headers;
        private readonly IPublisherSerializer _serializer;

        public Message(object message, IPublisherSerializer serializer, IRouting routing, string correlationId = default, CancellationToken cancellationToken = default)
        {
            CreatedAt = DateTime.UtcNow;
            Routing = routing ?? throw new ArgumentNullException(nameof(routing));
            CancellationToken = cancellationToken;
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            _message = message;
            _headers = new Dictionary<string, object>()
            {
                {nameof(Routing.ExchangeName), Routing.ExchangeName},
                {nameof(Routing.RoutingKey), Routing.RoutingKey},
                {"SerializerType", serializer.GetType().FullName},
                {"CreatedAt", CreatedAt.ToString("s")}
            };
        }

        public DateTime CreatedAt { get; }
        public string CorrelationId { get; }
        public IRouting Routing { get; }
        public CancellationToken CancellationToken { get; }

        public ReadOnlyMemory<byte> Serialize()
        {
            return _serializer.Serialize(_message);
        }
        
        public IDictionary<string, object> GetHeaders()
        {
            return _headers;
        }

        public void AddHeader(string key, object value)
        {
            if (_headers.ContainsKey(key))
            {
                _headers[key] = value;
                return;
            }
            
            _headers.Add(key, value);
        }
    }
}