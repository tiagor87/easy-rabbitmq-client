using System.Net.Mime;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Extensions
{
    public static class BasicPropertiesExtensions
    {
        public static void AddMessageProperties(this IBasicProperties basicProperties,
            IPublisherMessage publisherMessage)
        {
            basicProperties.ContentType = MediaTypeNames.Application.Json;
            basicProperties.CorrelationId = publisherMessage.CorrelationId;
            basicProperties.Headers = publisherMessage.GetHeaders();
        }
    }
}