using System;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Core.Exceptions
{
    public class EasyRabbitMqClientException : Exception
    {
        protected EasyRabbitMqClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EasyRabbitMqClientException(IPublisherMessageBatching batching, string message, Exception exception)
        {
            Batching = batching;
        }

        public IPublisherMessageBatching Batching { get; }
    }
}