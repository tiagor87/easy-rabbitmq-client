using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Abstractions.Subscribers.Models
{
    public interface ISubscriberMessage
    {
        string CorrelationId { get; }
        IRouting Routing { get; }
        IDictionary<string, object> Headers { get; }
        T GetValue<T>();
        Task AckAsync(CancellationToken cancellationToken);
        Task NotAckAsync(CancellationToken cancellationToken);
    }
}