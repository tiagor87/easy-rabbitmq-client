using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Publisher.Behaviors
{
    public class PublisherBehaviorWrapper : IBehavior<IPublisherMessageBatching>
    {
        private readonly IPublisherBehavior _publisher;
        private bool _disposed;

        public PublisherBehaviorWrapper(IPublisherBehavior publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task ExecuteAsync(IPublisherMessageBatching batching,
            Func<IPublisherMessageBatching, CancellationToken, Task> _, CancellationToken cancellationToken)
        {
            await _publisher.PublishAsync(batching, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PublisherBehaviorWrapper()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing) _publisher.Dispose();

            _disposed = true;
        }
    }
}