using System.Threading;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;

namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IMessageBuilder<TMessage> where TMessage : IMessage
    {
        IMessagePublisher Publisher { get; }
        IMessageBuilder<TMessage> ForPublisher(IMessagePublisher publisher);
        IMessageBuilder<TMessage> WithMessage(object message);
        IMessageBuilder<TMessage> WithCorrelationId(string correlationId);
        IMessageBuilder<TMessage> WithCancellationToken(CancellationToken cancellationToken);
        IMessageBuilder<TMessage> WithSerializer(IPublisherSerializer serializer);
        IMessageBuilder<TMessage> WithRouting(string exchangeName, string routingKey);
        TMessage Build();
    }
}