using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Publishers
{
    public interface IMessagePublisher : IObservable<IMessageBatching>, IDisposable
    {
        Task PublishAsync(IMessage message, CancellationToken cancellationToken);
        Task PublishBatchingAsync(IMessageBatching messageBatching, CancellationToken cancellationToken);
    }
}