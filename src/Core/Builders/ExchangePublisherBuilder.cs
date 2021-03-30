using EasyRabbitMqClient.Abstractions.Builders;

namespace EasyRabbitMqClient.Core.Builders
{
    public class ExchangePublisherBuilder<T> : ExchangeBuilder, IExchangePublisherBuilder<T>
    {
        private readonly IMessagePublisherBuilder<T> _publisherBuilder;

        public ExchangePublisherBuilder(IMessagePublisherBuilder<T> publisherBuilder)
        {
            _publisherBuilder = publisherBuilder;
        }
        
        public IMessagePublisherBuilder<T> BuildAndReturn()
        {
            var exchange = Build();
            return _publisherBuilder.WithExchange(exchange);
        }
    }
}