using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;

namespace EasyRabbitMqClient.Core.Behaviors
{
    public class AggregateBehavior<T> : IBehavior<T>
    {
        private readonly IBehavior<T> _behavior;
        private readonly IBehavior<T> _nextBehavior;
        private bool _disposed;

        private AggregateBehavior(IBehavior<T> behavior, IBehavior<T> nextBehavior)
        {
            _behavior = behavior;
            _nextBehavior = nextBehavior;
        }

        public async Task ExecuteAsync(T message, Func<T, CancellationToken, Task> next,
            CancellationToken cancellationToken)
        {
            async Task NextAction(T batch, CancellationToken ct)
            {
                await _nextBehavior.ExecuteAsync(batch, next, ct);
            }

            await _behavior.ExecuteAsync(message, NextAction, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AggregateBehavior()
        {
            Dispose(false);
        }

        public static IBehavior<T> Create(IBehavior<T> mainBehavior, params IBehavior<T>[] behaviors)
        {
            return Create(mainBehavior, behaviors.ToList());
        }

        public static IBehavior<T> Create(IBehavior<T> mainBehavior, IEnumerable<IBehavior<T>> behaviors)
        {
            return behaviors.Aggregate(mainBehavior,
                (nextBehavior, behavior) => new AggregateBehavior<T>(behavior, nextBehavior));
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _behavior?.Dispose();
                _nextBehavior?.Dispose();
            }

            _disposed = true;
        }
    }
}