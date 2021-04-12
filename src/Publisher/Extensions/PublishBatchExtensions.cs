using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Extensions;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Extensions
{
    internal static class PublishBatchExtensions
    {
        internal static bool Add(this IBasicPublishBatch batch, IModel model, IMessage message)
        {
            if (message.CancellationToken.IsCancellationRequested) return false;
            
            message.Routing.DeclareExchange(model);
            
            var basicProperties = model.CreateBasicProperties();
            basicProperties.AddMessageProperties(message);
            
            batch.Add(message.Routing.ExchangeName, message.Routing.RoutingKey, false, basicProperties,
                message.Serialize());
            
            return true;
        }
    }
}