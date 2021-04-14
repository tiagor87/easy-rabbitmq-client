using System;

namespace EasyRabbitMqClient.Behaviors.Retry
{
    public class GeometricBehavior : RetryBehaviorBase
    {
        private readonly int _coefficient;
        private readonly TimeSpan _delay;
        private readonly TimeSpan? _maxDelay;
        public GeometricBehavior(int coefficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null) : base(maxAttempts)
        {
            _coefficient = coefficient;
            _delay = delay;
            _maxDelay = maxDelay;
        }

        protected override TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(_delay.TotalMilliseconds * Math.Pow(_coefficient, attempt - 1));
            
            if (!_maxDelay.HasValue || delay < _maxDelay) return delay;
            
            return _maxDelay.Value;
        }
    }
}