using System;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Core.Models
{
    public class Routing : IRouting
    {
        public Routing(string exchangeName, string routingKey)
        {
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
        }

        public string ExchangeName { get; }
        public string RoutingKey { get; }
    }
}