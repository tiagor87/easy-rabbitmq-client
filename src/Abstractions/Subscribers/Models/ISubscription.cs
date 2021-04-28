namespace EasyRabbitMqClient.Abstractions.Subscribers.Models
{
    public interface ISubscription
    {
        string QueueName { get; }
        ushort PrefetchCount { get; }
        bool AutoAck { get; }
        int ReconnectDelayInMs { get; }
    }
}