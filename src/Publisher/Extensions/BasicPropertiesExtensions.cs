using System.Net.Mime;
using EasyRabbitMqClient.Abstractions.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Extensions
{
    public static class BasicPropertiesExtensions
    {
        public static void AddMessageProperties(this IBasicProperties basicProperties, IMessage message)
        {
            basicProperties.ContentType = MediaTypeNames.Application.Json;
            basicProperties.CorrelationId = message.CorrelationId;
            basicProperties.Headers = message.GetHeaders();
        }
    }
}