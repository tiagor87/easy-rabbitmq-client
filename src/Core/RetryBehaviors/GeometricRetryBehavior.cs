using System;
using EasyRabbitMqClient.Abstractions.RetryBehaviors;
using Polly;
using Polly.Retry;

namespace EasyRabbitMqClient.Core.RetryBehaviors
{
    public class GeometricRetryBehavior : IRetryBehavior
    {
        private RetryPolicy<bool> _retryPolicy;
        private int _coeficient;
        private TimeSpan _delay;
        private TimeSpan? _maxDelay;
        private int _maxAttempts;

        public GeometricRetryBehavior(int coeficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null)
        {
            _coeficient = coeficient;
            _delay = delay;
            _maxDelay = maxDelay;
            _maxAttempts = maxAttempts ?? int.MaxValue;
            _retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<bool>(x => !x)
                .WaitAndRetry(_maxAttempts, GetDelayForAttempt);
        }

        private TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds * Math.Pow(_coeficient, attempt - 1));
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }

        public bool Execute(Func<bool> action)
        {
            return _retryPolicy.Execute(action);
        }
    }
}