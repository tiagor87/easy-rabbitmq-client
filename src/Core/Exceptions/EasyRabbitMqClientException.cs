using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyRabbitMqClient.Core.Exceptions
{
    public class EasyRabbitMqClientException : Exception
    {
        protected EasyRabbitMqClientException(string message, Exception innerException) : base(message, innerException)
        {
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
    }
}