using System;

namespace EasyRabbitMqClient.Abstractions.Subscribers
{
    public interface ISubscriberSerializer
    {
        T Deserialize<T>(ReadOnlyMemory<byte> body);
    }
}