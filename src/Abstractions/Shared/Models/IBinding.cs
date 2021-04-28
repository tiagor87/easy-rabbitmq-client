namespace EasyRabbitMqClient.Abstractions.Shared.Models
{
    public interface IBinding
    {
        public string ExchangeName { get; }
        public string QueueName { get; }
        public string RoutingKey { get; }
    }
}