using System;
using EasyRabbitMqClient.Abstractions.Shared.Models;

namespace EasyRabbitMqClient.Subscriber.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BindingAttribute : Attribute, IBinding
    {
        public BindingAttribute(string exchangeName, string queueName, string routingKey)
        {
            ExchangeName = exchangeName;
            QueueName = queueName;
            RoutingKey = routingKey;
        }

        public string ExchangeName { get; }
        public string QueueName { get; }
        public string RoutingKey { get; }
    }
}