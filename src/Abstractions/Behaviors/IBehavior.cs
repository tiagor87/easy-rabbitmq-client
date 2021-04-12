using System;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface IBehavior
    {
        bool Execute(Func<bool> action);
    }
}