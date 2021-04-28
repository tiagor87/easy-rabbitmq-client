using System.Threading;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Abstractions.Publishers.Builders
{
    public interface IPublisherMessageBuilder<TMessage> where TMessage : IPublisherMessage
    {
        IPublisher Publisher { get; }
        IPublisherMessageBuilder<TMessage> ForPublisher(IPublisher publisher);
        IPublisherMessageBuilder<TMessage> WithMessage(object message);
        IPublisherMessageBuilder<TMessage> WithCorrelationId(string correlationId);
        IPublisherMessageBuilder<TMessage> WithCancellationToken(CancellationToken cancellationToken);
        IPublisherMessageBuilder<TMessage> WithSerializer(IPublisherSerializer serializer);
        IPublisherMessageBuilder<TMessage> WithRouting(string exchangeName, string routingKey);
        TMessage Build();
    }
}