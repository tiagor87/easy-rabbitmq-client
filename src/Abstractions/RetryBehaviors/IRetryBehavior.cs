using System;

namespace EasyRabbitMqClient.Abstractions.RetryBehaviors
{
    public interface IRetryBehavior
    {
        bool Execute(Func<bool> action);
    }
}