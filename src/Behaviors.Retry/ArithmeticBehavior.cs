using System;
using EasyRabbitMqClient.Abstractions.Behaviors;
using Polly;
using Polly.Retry;

namespace EasyRabbitMqClient.Behaviors.Retry
{
    public class ArithmeticBehavior : IBehavior
    {
        private readonly RetryPolicy<bool> _retryPolicy;
        private readonly int _coefficient;
        private readonly TimeSpan _delay;
        private readonly TimeSpan? _maxDelay;

        public ArithmeticBehavior(int coefficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null)
        {
            _coefficient = coefficient;
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
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds + (attempt - 1) * _coefficient);
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }
    }
}