using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyRabbitMqClient.Core.Extensions
{
    public static class ExchangeExtensions
    {
        public static void Declare(this IExchange exchange, IModel model)
        {
            try
            {
                model.ExchangeDeclare(exchange.Name, exchange.Type, exchange.IsDurable, exchange.IsAutoDelete);
            }
            catch (OperationInterruptedException ex)
            {
                throw EasyRabbitMqClientException.Create(ex);
            }
        }
    }
}