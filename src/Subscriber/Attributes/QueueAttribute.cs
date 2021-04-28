using System;
using EasyRabbitMqClient.Abstractions.Shared.Models;

namespace EasyRabbitMqClient.Subscriber.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueAttribute : Attribute, IQueue
    {
        public QueueAttribute(string name, bool durable = true, bool autoDelete = false, bool exclusive = false)
        {
            Name = name;
            Durable = durable;
            AutoDelete = autoDelete;
            Exclusive = exclusive;
        }

        public string Name { get; }
        public bool Durable { get; }
        public bool AutoDelete { get; }
        public bool Exclusive { get; }
    }
}