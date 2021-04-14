using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Core.Behaviors
{
    public class AggregateBehavior : IBehavior
    {
        private bool _disposed;
        private readonly IBehavior _behavior;
        private readonly IBehavior _nextBehavior;

        private AggregateBehavior(IBehavior behavior, IBehavior nextBehavior)
        {
            _behavior = behavior;
            _nextBehavior = nextBehavior;
        }
            
        ~AggregateBehavior()
        {
            Dispose(false);
        }
        
        public static IBehavior Create(IBehavior mainBehavior, params IBehavior[] behaviors)
        {
            return behaviors.Aggregate(mainBehavior, (nextBehavior, behavior) => new AggregateBehavior(behavior, nextBehavior));
        }
            
        public async Task ExecuteAsync(IMessageBatching batching, Func<IMessageBatching, CancellationToken, Task> next, CancellationToken cancellationToken)
        {
            async Task NextAction(IMessageBatching batch, CancellationToken ct) =>
                await _nextBehavior.ExecuteAsync(batch, next, ct);
                
            await _behavior.ExecuteAsync(batching, NextAction,  cancellationToken);
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
                _behavior?.Dispose();
                _nextBehavior?.Dispose();
            }

            _disposed = true;
        }
    }
}