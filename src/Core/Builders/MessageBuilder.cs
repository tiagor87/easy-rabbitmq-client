using System;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Models;

namespace EasyRabbitMqClient.Core.Builders
{
    public class MessageBuilder : IMessageBuilder<IPublishingMessage>
    {
        private IMessagePublisher _publisher;
        private object _message;
        private CancellationToken _cancellationToken;
        private IPublisherSerializer _serializer;
        private IRouting _routing;
        private string _correlationId;
        
        public IMessagePublisher Publisher => _publisher;

        public IMessageBuilder<IPublishingMessage> ForPublisher(IMessagePublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            return this;
        }

        public IMessageBuilder<IPublishingMessage> WithMessage(object message)
        {
            _message = message;
            return this;
        }

        public IMessageBuilder<IPublishingMessage> WithCorrelationId(string correlationId)
        {
            _correlationId = correlationId;
            return this;
        }

        public IMessageBuilder<IPublishingMessage> WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public IMessageBuilder<IPublishingMessage> WithSerializer(IPublisherSerializer serializer)
        {
            _serializer = serializer;
            return this;
        }

        public IMessageBuilder<IPublishingMessage> WithRouting(string exchangeName, string routingKey)
        {
            _routing = new Routing(exchangeName, routingKey);
            return this;
        }

        public IPublishingMessage Build()
        {
            var message = new PublishingMessage(_publisher, _message, _serializer, _routing, _correlationId, _cancellationToken);
            return message;
        }
    }
}