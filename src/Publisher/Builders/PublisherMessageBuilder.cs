using System;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Builders;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Publisher.Models;

namespace EasyRabbitMqClient.Publisher.Builders
{
    public class PublisherMessageBuilder : IPublisherMessageBuilder<IPublisherMessage>
    {
        private CancellationToken _cancellationToken;
        private string _correlationId;
        private object _message;
        private IRouting _routing;
        private IPublisherSerializer _serializer;

        public IPublisher Publisher { get; private set; }

        public IPublisherMessageBuilder<IPublisherMessage> ForPublisher(IPublisher publisher)
        {
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            return this;
        }

        public IPublisherMessageBuilder<IPublisherMessage> WithMessage(object message)
        {
            _message = message;
            return this;
        }

        public IPublisherMessageBuilder<IPublisherMessage> WithCorrelationId(string correlationId)
        {
            _correlationId = correlationId;
            return this;
        }

        public IPublisherMessageBuilder<IPublisherMessage> WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public IPublisherMessageBuilder<IPublisherMessage> WithSerializer(IPublisherSerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public IPublisherMessageBuilder<IPublisherMessage> WithRouting(string exchangeName, string routingKey)
        {
            _routing = new Routing(exchangeName, routingKey);
            return this;
        }

        public IPublisherMessage Build()
        {
            var message = new PublisherMessage(Publisher, _message, _serializer, _routing, _correlationId,
                _cancellationToken);
            return message;
        }
    }
}