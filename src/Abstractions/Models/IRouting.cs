namespace EasyRabbitMqClient.Abstractions.Models
{
    public interface IRouting
    {
        string ExchangeName { get; }
        string RoutingKey { get; }
    }
}