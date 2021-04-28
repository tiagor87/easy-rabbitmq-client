using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;

namespace EasyRabbitMqClient.Subscriber.Behaviors
{
    public class SubscriberBehaviorWrapper<T> : IBehavior<ISubscriberMessage>
    {
        private readonly ISubscriberHandler<T> _subscriber;
        private bool _disposed;

        public SubscriberBehaviorWrapper(ISubscriberHandler<T> subscriber)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        public async Task ExecuteAsync(ISubscriberMessage message, Func<ISubscriberMessage, CancellationToken, Task> _,
            CancellationToken cancellationToken)
        {
            await _subscriber.HandleAsync(message.GetValue<T>(), cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SubscriberBehaviorWrapper()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Nothing to do
            }

            _disposed = true;
        }
    }
}