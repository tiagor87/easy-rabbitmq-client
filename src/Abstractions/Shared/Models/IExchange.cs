namespace EasyRabbitMqClient.Abstractions.Shared.Models
{
    public interface IExchange
    {
        public string Name { get; }
        public ExchangeType Type { get; }
        public bool Durable { get; }
        public bool AutoDelete { get; }
    }
}