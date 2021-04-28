using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Publisher.Models
{
    public class PublisherMessage : IPublisherMessage
    {
        private readonly IDictionary<string, object> _headers;
        private readonly object _message;
        private readonly IPublisherSerializer _serializer;

        public PublisherMessage(
            IPublisher publisher,
            object message,
            IPublisherSerializer serializer,
            IRouting routing,
            string correlationId = default,
            CancellationToken cancellationToken = default)
        {
            CreatedAt = DateTime.UtcNow;
            Routing = routing ?? throw new ArgumentNullException(nameof(routing));
            CancellationToken = cancellationToken;
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            _message = message;
            _headers = new Dictionary<string, object>
            {
                {nameof(Routing.ExchangeName), Routing.ExchangeName},
                {nameof(Routing.RoutingKey), Routing.RoutingKey},
                {"SerializerType", serializer.GetType().FullName},
                {"CreatedAt", CreatedAt.ToString("s")}
            };
        }

        public IPublisher Publisher { get; }
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

        public async Task PublishAsync(CancellationToken cancellationToken)
        {
            MarkAsPublished();
            await Publisher.PublishAsync(this, cancellationToken);
        }

        public void MarkAsPublished()
        {
            AddHeader("PublishedAt", DateTime.UtcNow.ToString("s"));
        }
    }
}