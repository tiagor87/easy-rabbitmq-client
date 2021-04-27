using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface IPublisherBehavior : IDisposable
    {
        Task PublishAsync(IMessageBatching batching, CancellationToken cancellationToken);
    }
}