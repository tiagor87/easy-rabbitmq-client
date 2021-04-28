using System;
using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions.Shared.Models;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;

namespace EasyRabbitMqClient.Abstractions.Subscribers
{
    public interface ISubscriber : IDisposable
    {
        IDisposable Subscribe<T, TMessage>(
            ISubscription subscription,
            IExchange exchange = null,
            IQueue queue = null,
            IEnumerable<IBinding> bindings = null) where T : ISubscriberHandler<TMessage>;

        IDisposable Subscribe<T, TMessage>() where T : ISubscriberHandler<TMessage>;
    }
}