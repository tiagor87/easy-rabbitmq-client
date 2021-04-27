using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Publisher.Behaviors
{
    public class PublisherBehaviorWrapper : IBehavior
    {
        private readonly IPublisherBehavior _publisher;
        private bool _disposed;

        public PublisherBehaviorWrapper(IPublisherBehavior publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        ~PublisherBehaviorWrapper()
        {
            Dispose(false);
        }
        
        public async Task ExecuteAsync(IMessageBatching batching, Func<IMessageBatching, CancellationToken, Task> _, CancellationToken cancellationToken)
        {
            await _publisher.PublishAsync(batching, cancellationToken);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _publisher.Dispose();
            }

            _disposed = true;
        }
    }
}