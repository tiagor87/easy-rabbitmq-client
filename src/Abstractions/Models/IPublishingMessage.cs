using System.Threading;
using System.Threading.Tasks;

namespace EasyRabbitMqClient.Abstractions.Models
{
    public interface IPublishingMessage : IMessage
    {
        Task PublishAsync(CancellationToken cancellationToken);
    }
}