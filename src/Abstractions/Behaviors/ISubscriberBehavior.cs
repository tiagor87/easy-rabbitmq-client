using EasyRabbitMqClient.Abstractions.Subscribers.Models;

namespace EasyRabbitMqClient.Abstractions.Behaviors
{
    public interface ISubscriberBehavior : IBehavior<ISubscriberMessage>
    {
    }
}