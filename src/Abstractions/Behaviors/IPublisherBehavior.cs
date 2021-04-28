using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface IPublisherBehavior : IDisposable
    {
        Task PublishAsync(IPublisherMessageBatching batching, CancellationToken cancellationToken);
    }
}