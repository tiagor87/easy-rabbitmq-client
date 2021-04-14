using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public abstract class PublisherBehaviorBase : IPublisherBehavior
    {
        private bool _disposed;
        
        public async Task ExecuteAsync(IMessageBatching batching, Func<IMessageBatching, CancellationToken, Task> _, CancellationToken cancellationToken)
        {
            await PublishAsync(batching, cancellationToken);
        }

        ~PublisherBehaviorBase()
        {
            Dispose(false);
        }

        protected abstract Task PublishAsync(IMessageBatching batching, CancellationToken cancellationToken);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void OnDispose();
        
        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                OnDispose();
            }

            _disposed = true;
        }
    }
}