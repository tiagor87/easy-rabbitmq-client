using System;

namespace EasyRabbitMqClient.Core.Exceptions
{
    public class ForbiddenException : EasyRabbitMqClientException
    {
        public ForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}