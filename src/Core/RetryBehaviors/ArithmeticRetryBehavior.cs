using System;
using EasyRabbitMqClient.Abstractions.RetryBehaviors;
using Polly;
using Polly.Retry;

namespace EasyRabbitMqClient.Core.RetryBehaviors
{
    public class ArithmeticRetryBehavior : IRetryBehavior
    {
        private readonly RetryPolicy<bool> _retryPolicy;
        private readonly int _coeficient;
        private readonly TimeSpan _delay;
        private readonly TimeSpan? _maxDelay;

        public ArithmeticRetryBehavior(int coeficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null)
        {
            _coeficient = coeficient;
            _delay = delay;
            _maxDelay = maxDelay;
            _retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<bool>(x => !x)
                .WaitAndRetry(maxAttempts ?? int.MaxValue, GetDelayForAttempt);
        }

        public bool Execute(Func<bool> action)
        {
            return _retryPolicy.Execute(action);
        }
        
        private TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds + (attempt - 1) * _coeficient);
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }
    }
}