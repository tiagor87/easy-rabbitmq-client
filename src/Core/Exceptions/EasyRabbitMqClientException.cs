using System;
using EasyRabbitMqClient.Abstractions.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyRabbitMqClient.Core.Exceptions
{
    public class EasyRabbitMqClientException : Exception
    {
        protected EasyRabbitMqClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EasyRabbitMqClientException(IMessageBatching batching, string message, Exception exception)
        {
            Batching = batching;
        }

        public static EasyRabbitMqClientException Create(OperationInterruptedException ex)
        {
            return ex.ShutdownReason.ReplyCode switch
            {
                Constants.AccessRefused => new ForbiddenException(ex.ShutdownReason.ReplyText, ex),
                Constants.NotFound => new NotFoundException(ex.ShutdownReason.ReplyText, ex),
                _ => new EasyRabbitMqClientException(ex.ShutdownReason.ReplyText, ex)
            };
        }

        public IMessageBatching Batching { get; }
    }
}