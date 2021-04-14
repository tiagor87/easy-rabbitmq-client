using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface IBehavior : IDisposable
    {
        Task ExecuteAsync(IMessageBatching batching, Func<IMessageBatching, CancellationToken, Task> next,
            CancellationToken cancellationToken);
    }
}