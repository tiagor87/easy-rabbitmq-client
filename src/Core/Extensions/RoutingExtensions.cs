using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyRabbitMqClient.Core.Extensions
{
    public static class RoutingExtensions
    {
        private static readonly HashSet<string> _declaredExchanges = new();
        private static readonly object _sync = new object();
        
        public static void DeclareExchange(this IRouting routing, IModel model)
        {
            try
            {
                if (_declaredExchanges.Contains(routing.ExchangeName)) return;
                lock (_sync)
                {
                    if (_declaredExchanges.Contains(routing.ExchangeName)) return;
                    model.ExchangeDeclarePassive(routing.ExchangeName);
                    _declaredExchanges.Add(routing.ExchangeName);
                }
            }
            catch (OperationInterruptedException ex)
            {
                throw EasyRabbitMqClientException.Create(ex);
            }
        }
    }
}