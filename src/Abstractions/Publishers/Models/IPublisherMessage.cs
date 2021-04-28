using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions.Publishers.Models
{
    public interface IPublisherMessage
    {
        DateTime CreatedAt { get; }
        string CorrelationId { get; }
        IRouting Routing { get; }
        CancellationToken CancellationToken { get; }
        IPublisher Publisher { get; }
        ReadOnlyMemory<byte> Serialize();
        IDictionary<string, object> GetHeaders();
        void AddHeader(string key, object value);
        void MarkAsPublished();
        Task PublishAsync(CancellationToken cancellationToken);
    }
}