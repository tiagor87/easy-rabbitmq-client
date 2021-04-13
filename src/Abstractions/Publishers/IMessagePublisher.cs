using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Publishers
{
    public interface IMessagePublisher : IObservable<IMessageBatching>, IDisposable
    {
        IMessageBuilder<IPublishingMessage> NewMessage();
        Task PublishAsync(IMessage message, CancellationToken cancellationToken);
        Task PublishBatchingAsync(IMessageBatching messageBatching, CancellationToken cancellationToken);
    }
}