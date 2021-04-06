using System;

namespace EasyRabbitMqClient.Abstractions.RetryBehaviors
{
    public interface IRetryBehavior
    {
        public int Coeficient { get; }
        public TimeSpan Delay { get; }
        public int MaxAttempts { get; }
        public TimeSpan? MaxDelay { get; }
        bool ShouldRetry(Exception exception);
        TimeSpan GetDelayForAttempt(int attempt);
    }
}