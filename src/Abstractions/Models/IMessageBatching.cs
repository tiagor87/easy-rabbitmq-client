using System;
using System.Collections.Generic;

namespace EasyRabbitMqClient.Abstractions.Models
{
    public interface IMessageBatching : IReadOnlyCollection<IMessage>
    {
        TimeSpan PublishingTimeout { get; }
    }
}