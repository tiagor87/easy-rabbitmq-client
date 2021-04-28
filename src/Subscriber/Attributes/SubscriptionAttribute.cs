using System;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;

namespace EasyRabbitMqClient.Subscriber.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SubscriptionAttribute : Attribute, ISubscription
    {
        public SubscriptionAttribute(string queueName, ushort prefetchCount, bool autoAck = true,
            int reconnectDelayInMs = 100)
        {
            QueueName = queueName;
            PrefetchCount = prefetchCount;
            AutoAck = autoAck;
            ReconnectDelayInMs = reconnectDelayInMs;
        }

        public string QueueName { get; }
        public ushort PrefetchCount { get; }
        public bool AutoAck { get; }
        public int ReconnectDelayInMs { get; }

        public override int GetHashCode()
        {
            return QueueName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SubscriptionAttribute attribute)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return QueueName == attribute.QueueName;
        }
    }
}