namespace EasyRabbitMqClient.Abstractions.Models
{
    public interface IExchange
    {
        string Name { get; }
        string Type { get; }
        bool IsDurable { get; }
        bool IsAutoDelete { get; }
        bool IsInternal { get; }
        IExchange FallbackExchange { get; }
    }
}