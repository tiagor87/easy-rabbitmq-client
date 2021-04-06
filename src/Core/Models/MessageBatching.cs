using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Core.Models
{
    public class MessageBatching : ReadOnlyCollection<IMessage>, IMessageBatching
    {
        public MessageBatching(IEnumerable<IMessage> messages) : base(messages.ToList())
        {
        }
    }
}