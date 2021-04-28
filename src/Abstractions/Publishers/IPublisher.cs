using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers.Builders;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Abstractions.Publishers
{
    public interface IPublisher : IPublisherBehavior, IObservable<IPublisherMessageBatching>
    {
        IPublisherMessageBuilder<IPublisherMessage> NewMessage();
        Task PublishAsync(IPublisherMessage publisherMessage, CancellationToken cancellationToken);
    }
}