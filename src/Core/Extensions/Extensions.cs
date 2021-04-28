using System;
using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Abstractions.Shared.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core.Extensions
{
    public static class Extensions
    {
        private static readonly HashSet<string> _declaredExchanges = new HashSet<string>();

        public static void Verify(this IRouting routing, IModel model)
        {
            if (_declaredExchanges.Contains(routing.ExchangeName)) return;
            model.ExchangeDeclarePassive(routing.ExchangeName);
            _declaredExchanges.Add(routing.ExchangeName);
        }

        public static void Declare(this IExchange exchange, IModel channel)
        {
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            channel.ExchangeDeclare(
                exchange.Name,
                exchange.Type.ToString().ToLowerInvariant(),
                exchange.Durable,
                exchange.AutoDelete);
        }

        public static void Declare(this IQueue queue, IModel channel)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            channel.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete);
        }

        public static void Bind(this IBinding binding, IModel channel)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            channel.QueueBind(binding.QueueName, binding.ExchangeName, binding.RoutingKey);
        }

        public static void Bind(this IEnumerable<IBinding> bindings, IModel channel)
        {
            if (bindings == null) throw new ArgumentNullException(nameof(bindings));
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            foreach (var binding in bindings) binding.Bind(channel);
        }
    }
}