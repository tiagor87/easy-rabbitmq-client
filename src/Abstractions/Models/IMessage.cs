using System;
using System.Collections.Generic;
using System.Threading;

namespace EasyRabbitMqClient.Abstractions.Models
{
    public interface IMessage
    {
        DateTime CreatedAt { get; }
        string CorrelationId { get; }
        IRouting Routing { get; }
        CancellationToken CancellationToken { get; }
        ReadOnlyMemory<byte> Serialize();
        IDictionary<string, object> GetHeaders();
        void AddHeader(string key, object value);
    }
}