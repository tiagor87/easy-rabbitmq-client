using System;
using EasyRabbitMqClient.Abstractions.Shared.Models;

namespace EasyRabbitMqClient.Subscriber.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExchangeAttribute : Attribute, IExchange
    {
        public ExchangeAttribute(string name, ExchangeType type, bool durable = true, bool autoDelete = false)
        {
            Name = name;
            Type = type;
            Durable = durable;
            AutoDelete = autoDelete;
        }

        public string Name { get; }
        public ExchangeType Type { get; }
        public bool Durable { get; }
        public bool AutoDelete { get; }
    }
}