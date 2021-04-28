using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Extensions;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Extensions
{
    internal static class PublishBatchExtensions
    {
        internal static bool Add(this IBasicPublishBatch batch, IModel model, IPublisherMessage publisherMessage)
        {
            if (publisherMessage.CancellationToken.IsCancellationRequested) return false;

            publisherMessage.Routing.Verify(model);

            publisherMessage.MarkAsPublished();

            var basicProperties = model.CreateBasicProperties();
            basicProperties.AddMessageProperties(publisherMessage);

            batch.Add(publisherMessage.Routing.ExchangeName, publisherMessage.Routing.RoutingKey, false,
                basicProperties,
                publisherMessage.Serialize());

            return true;
        }
    }
}