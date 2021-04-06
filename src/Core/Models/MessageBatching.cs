using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Core.Models
{
    public class MessageBatching : ReadOnlyCollection<IMessage>, IMessageBatching
    {
        private const int TIMEOUT = 500;
        
        public MessageBatching(IEnumerable<IMessage> messages, TimeSpan? publishingTimeout = null) : base(messages.ToList())
        {
            PublishingTimeout = publishingTimeout ?? TimeSpan.FromMilliseconds(TIMEOUT);
        }

        public TimeSpan PublishingTimeout { get; }
    }
}