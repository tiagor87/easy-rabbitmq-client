using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface IBehavior<T> : IDisposable
    {
        Task ExecuteAsync(T message, Func<T, CancellationToken, Task> next,
            CancellationToken cancellationToken);
    }
}