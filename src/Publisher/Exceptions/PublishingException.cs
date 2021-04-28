using System;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Exceptions;

namespace EasyRabbitMqClient.Publisher.Exceptions
{
    public class PublishingException : EasyRabbitMqClientException
    {
        public PublishingException(IPublisherMessageBatching batching, string message, Exception innerException) : base(
            batching, message, innerException)
        {
        }

        public PublishingException(IPublisherMessageBatching batching, Exception innerException) : this(batching,
            "Failed to publish batching.", innerException)
        {
        }
    }
}