using System;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Models;

namespace EasyRabbitMqClient.Core.Builders
{
    public class MessageBuilder : IMessageBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private object _message;
        private CancellationToken _cancellationToken;
        private IPublisherSerializer _serializer;
        private IRouting _routing;

        public MessageBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public IMessageBuilder WithMessage(object message)
        {
            _message = message;
            return this;
        }

        public IMessageBuilder WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public IMessageBuilder WithSerializer<T>() where T : IPublisherSerializer
        {
            _serializer = (IPublisherSerializer) _serviceProvider.GetService(typeof(T));
            return this;
        }

        public IMessageBuilder WithRouting(string exchangeName, string routingKey)
        {
            _routing = new Routing(exchangeName, routingKey);
            return this;
        }

        public IMessage Build()
        {
            var message = new Message(_message, _serializer, _routing, _cancellationToken);
            return message;
        }
    }
}