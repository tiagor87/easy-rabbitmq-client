using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions
{
    public interface IMessagePublisher<in T>
    {
        Task PublishAsync(T message, CancellationToken cancellationToken);
    }
}