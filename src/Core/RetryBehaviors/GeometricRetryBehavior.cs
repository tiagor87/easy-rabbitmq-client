using System;
using EasyRabbitMqClient.Abstractions.RetryBehaviors;

namespace EasyRabbitMqClient.Core.RetryBehaviors
{
    public class GeometricRetryBehavior : IRetryBehavior
    {
        public GeometricRetryBehavior(int coeficient, TimeSpan delay, int? maxAttempts = null, TimeSpan? maxDelay = null)
        {
            Coeficient = coeficient;
            Delay = delay;
            MaxDelay = maxDelay;
            MaxAttempts = maxAttempts ?? int.MaxValue;
        }

        public int Coeficient { get; }
        public TimeSpan Delay { get; }
        public int MaxAttempts { get; }
        public TimeSpan? MaxDelay { get; }

        public bool ShouldRetry(Exception exception)
        {
            return true;
        }

        public TimeSpan GetDelayForAttempt(int attempt)
        {
            var delay = TimeSpan.FromMilliseconds(Delay.TotalMilliseconds * Math.Pow(Coeficient, attempt - 1));
            
            if (!MaxDelay.HasValue || delay < MaxDelay) return delay;
            
            return MaxDelay.Value;
        }
    }
}