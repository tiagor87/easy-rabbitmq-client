namespace EasyRabbitMqClient.Abstractions.Shared.Models
{
    public interface IQueue
    {
        public string Name { get; }
        public bool Durable { get; }
        public bool AutoDelete { get; }
        public bool Exclusive { get; }
    }
}