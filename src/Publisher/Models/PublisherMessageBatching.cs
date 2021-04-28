using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Publisher.Models
{
    public class PublisherMessageBatching : ReadOnlyCollection<IPublisherMessage>, IPublisherMessageBatching
    {
        private const int TIMEOUT = 500;

        public PublisherMessageBatching(IPublisher publisher, params IPublisherMessage[] messages) :
            base(messages)
        {
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            PublishingTimeout = TimeSpan.FromMilliseconds(TIMEOUT);
        }

        public PublisherMessageBatching(IPublisher publisher, IEnumerable<IPublisherMessage> messages,
            TimeSpan? publishingTimeout = null) : base(messages.ToList())
        {
            Publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            PublishingTimeout = publishingTimeout ?? TimeSpan.FromMilliseconds(TIMEOUT);
        }

        public TimeSpan PublishingTimeout { get; }
        public IPublisher Publisher { get; }

        public async Task PublishAsync(CancellationToken cancellationToken)
        {
            await Publisher.PublishAsync(this, cancellationToken);
        }
    }
}