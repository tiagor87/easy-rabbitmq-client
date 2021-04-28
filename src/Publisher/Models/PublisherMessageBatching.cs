using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Publisher.Models
{
    public class PublisherMessageBatching : ReadOnlyCollection<IPublisherMessage>, IPublisherMessageBatching
    {
        private const int TIMEOUT = 500;

        public PublisherMessageBatching(IEnumerable<IPublisherMessage> messages, TimeSpan? publishingTimeout = null) :
            base(messages.ToList())
        {
            PublishingTimeout = publishingTimeout ?? TimeSpan.FromMilliseconds(TIMEOUT);
        }

        public TimeSpan PublishingTimeout { get; }
    }
}