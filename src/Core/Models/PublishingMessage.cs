using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;

namespace EasyRabbitMqClient.Core.Models
{
    public class PublishingMessage : Message, IPublishingMessage
    {
        private readonly IMessagePublisher _publisher;

        public PublishingMessage(IMessagePublisher publisher, object message, IPublisherSerializer serializer, IRouting routing, string correlationId, CancellationToken cancellationToken)
            : base(message, serializer, routing, correlationId, cancellationToken)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }
        
        public async Task PublishAsync(CancellationToken cancellationToken)
        {
            await _publisher.PublishAsync(this, cancellationToken);
        }
    }
}