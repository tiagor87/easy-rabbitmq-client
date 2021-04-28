namespace EasyRabbitMqClient.Abstractions.Publishers.Models
{
    public interface IRouting
    {
        string ExchangeName { get; }
        string RoutingKey { get; }
    }
}