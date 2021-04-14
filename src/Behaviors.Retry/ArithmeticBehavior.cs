using System;

namespace EasyRabbitMqClient.Behaviors.Retry
{
    public class ArithmeticBehavior : RetryBehaviorBase
    {
        private readonly int _coefficient;
        private readonly TimeSpan _delay;
        private readonly TimeSpan? _maxDelay;

        public ArithmeticBehavior(int coefficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null) : base(maxAttempts)
        {
            _coefficient = coefficient;
            _delay = delay;
            _maxDelay = maxDelay;
        }
        
        protected override TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds + (attempt - 1) * _coefficient);
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }
    }
}