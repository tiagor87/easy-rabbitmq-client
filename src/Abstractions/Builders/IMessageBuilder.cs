using System.Threading;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;

namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IMessageBuilder
    {
        IMessageBuilder WithMessage(object message);
        IMessageBuilder WithCancellationToken(CancellationToken cancellationToken);
        IMessageBuilder WithSerializer<T>() where T : IPublisherSerializer;
        IMessageBuilder WithRouting(string exchangeName, string routingKey);
        IMessage Build();
    }
}