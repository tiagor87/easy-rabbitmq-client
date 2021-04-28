using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Exceptions;
using Polly;
using Polly.Retry;

namespace EasyRabbitMqClient.Behaviors.Retry
{
    public abstract class RetryBehaviorBase : IBehavior<IPublisherMessageBatching>
    {
        private readonly AsyncRetryPolicy _retryPolicy;
        private bool _disposed;

        protected RetryBehaviorBase(int? maxAttempts = null)
        {
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxAttempts ?? int.MaxValue, GetDelayForAttempt);
        }

        public async Task ExecuteAsync(IPublisherMessageBatching batching,
            Func<IPublisherMessageBatching, CancellationToken, Task> next, CancellationToken cancellationToken)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await next(batching, cancellationToken);
                }
                catch (EasyRabbitMqClientException ex)
                {
                    batching = ex.Batching;
                    throw;
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract TimeSpan GetDelayForAttempt(int arg);

        protected virtual void OnDispose()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing) OnDispose();

            _disposed = true;
        }
    }
}