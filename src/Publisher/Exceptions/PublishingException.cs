using System;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Exceptions;

namespace EasyRabbitMqClient.Publisher.Exceptions
{
    public class PublishingException : EasyRabbitMqClientException
    {
        public PublishingException(IMessageBatching batching, string message, Exception innerException) : base(batching, message, innerException)
        {
        }
        
        public PublishingException(IMessageBatching batching, Exception innerException) : this(batching, "Failed to publish batching.", innerException)
        {
        }
    }
}