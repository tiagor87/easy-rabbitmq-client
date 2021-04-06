using System;

namespace EasyRabbitMqClient.Core.Exceptions
{
    public class NotFoundException : EasyRabbitMqClientException
    {
        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}