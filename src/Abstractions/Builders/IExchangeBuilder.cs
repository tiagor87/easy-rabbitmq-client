using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IExchangeBuilder
    {
        IExchangeBuilder WithName(string name);
        IExchangeBuilder WithFallbackExchange(string name);
        IExchangeBuilder AsDirect();
        IExchangeBuilder AsTopic();
        IExchangeBuilder AsFanOut();
        IExchangeBuilder AsDurable();
        IExchangeBuilder AsTransient();
        IExchangeBuilder WithSelfDestruction();
        IExchangeBuilder WithoutSelfDestruction();
        IExchangeBuilder AsInternal();
        IExchangeBuilder AsPublic();
        IExchange Build();
    }
}