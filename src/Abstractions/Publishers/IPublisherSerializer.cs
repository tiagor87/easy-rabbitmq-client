using System;

namespace EasyRabbitMqClient.Abstractions.Publishers
{
    public interface IPublisherSerializer : IDisposable
    {
        byte[] Serialize(object message);
    }
}