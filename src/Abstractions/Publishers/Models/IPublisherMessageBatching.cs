using System;
using System.Collections.Generic;

namespace EasyRabbitMqClient.Abstractions.Publishers.Models
{
    public interface IPublisherMessageBatching : IReadOnlyCollection<IPublisherMessage>
    {
        TimeSpan PublishingTimeout { get; }
    }
}