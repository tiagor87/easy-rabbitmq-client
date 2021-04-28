using System;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Exceptions;

namespace EasyRabbitMqClient.Publisher.Exceptions
{
    public class PublishingNotConfirmedException : EasyRabbitMqClientException
    {
        public PublishingNotConfirmedException(IPublisherMessageBatching batching, string message, Exception exception)
            : base(batching, message, exception)
        {
        }
    }
}