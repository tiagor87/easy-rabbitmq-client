using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions.Subscribers.Models
{
    public interface ISubscriberHandler<in T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}