using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core.Builders
{
    public class ExchangeBuilder : IExchangeBuilder
    {
        private string _name;
        private IExchange _fallbackExchange;
        private string _type;
        private bool _isDurable;
        private bool _autoDelete;

        public IExchangeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public IExchangeBuilder WithFallbackExchange(string name)
        {
            _fallbackExchange = new ExchangeBuilder()
                .AsDirect()
                .WithName(_name)
                .Build();
            return this;
        }

        public IExchangeBuilder AsDirect()
        {
            _type = ExchangeType.Direct;
            return this;
        }

        public IExchangeBuilder AsTopic()
        {
            _type = ExchangeType.Topic;
            return this;
        }

        public IExchangeBuilder AsFanOut()
        {
            _type = ExchangeType.Fanout;
            return this;
        }

        public IExchangeBuilder AsDurable()
        {
            _isDurable = true;
            return this;
        }

        public IExchangeBuilder AsTransient()
        {
            _isDurable = false;
            return this;
        }

        public IExchangeBuilder WithSelfDestruction()
        {
            _autoDelete = true;
            return this;
        }

        public IExchangeBuilder WithoutSelfDestruction()
        {
            _autoDelete = false;
            return this;
        }

        public IExchange Build()
        {
            return new Exchange(_name, _type, _isDurable, _autoDelete, _fallbackExchange);
        }
    }
}