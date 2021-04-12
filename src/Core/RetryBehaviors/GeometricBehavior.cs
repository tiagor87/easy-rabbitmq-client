using System;
using EasyRabbitMqClient.Abstractions.Behaviors;
using Polly;
using Polly.Retry;

namespace EasyRabbitMqClient.Core.RetryBehaviors
{
    public class GeometricBehavior : IBehavior
    {
        private readonly RetryPolicy<bool> _retryPolicy;
        private readonly int _coefficient;
        private readonly TimeSpan _delay;
        private readonly TimeSpan? _maxDelay;

        public GeometricBehavior(int coefficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null)
        {
            _coefficient = coefficient;
            _delay = delay;
            _maxDelay = maxDelay;
            _retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<bool>(x => !x)
                .WaitAndRetry(maxAttempts ?? int.MaxValue, GetDelayForAttempt);
        }

        private TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds * Math.Pow(_coefficient, attempt - 1));
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }

        public bool Execute(Func<bool> action)
        {
            return _retryPolicy.Execute(action);
        }
    }
}