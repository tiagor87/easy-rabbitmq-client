namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IExchangePublisherBuilder<T> : IExchangeBuilder
    {
        IMessagePublisherBuilder<T> BuildAndReturn();
    }
}