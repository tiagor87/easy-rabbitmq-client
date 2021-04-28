using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions.Publishers.Models
{
    public interface IPublisherMessageBatching : IReadOnlyCollection<IPublisherMessage>
    {
        TimeSpan PublishingTimeout { get; }
        IPublisher Publisher { get; }
        Task PublishAsync(CancellationToken cancellationToken);
    }
}