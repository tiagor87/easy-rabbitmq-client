using System;
using System.Collections.Generic;
using System.Reflection;
using EasyRabbitMqClient.Abstractions.Shared.Models;
using EasyRabbitMqClient.Abstractions.Subscribers;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;
using EasyRabbitMqClient.Core.Extensions;
using EasyRabbitMqClient.Core.Observers;
using EasyRabbitMqClient.Subscriber.Attributes;
using EasyRabbitMqClient.Subscriber.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Subscriber
{
    public class Subscriber : ISubscriber
    {
        private static volatile object _sync = new object();
        private readonly IConnectionFactory _connectionFactory;
        private readonly HashSet<IDisposable> _consumers;
        private readonly IServiceProvider _provider;
        private IConnection _connection;
        private bool _disposed;

        public Subscriber(IServiceProvider provider, IConnectionFactory connectionFactory)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _consumers = new HashSet<IDisposable>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDisposable Subscribe<T, TMessage>() where T : ISubscriberHandler<TMessage>
        {
            var subscriptionType = typeof(T);
            var exchange = subscriptionType.GetCustomAttribute<ExchangeAttribute>();
            var queue = subscriptionType.GetCustomAttribute<QueueAttribute>();
            var bindings = subscriptionType.GetCustomAttributes<BindingAttribute>();
            var subscription = subscriptionType.GetCustomAttribute<SubscriptionAttribute>();

            return Subscribe<T, TMessage>(subscription, exchange, queue, bindings);
        }

        public IDisposable Subscribe<T, TMessage>(
            ISubscription subscription,
            IExchange exchange = null,
            IQueue queue = null,
            IEnumerable<IBinding> bindings = null) where T : ISubscriberHandler<TMessage>
        {
            using var channel = Connect().CreateModel();
            exchange?.Declare(channel);
            queue?.Declare(channel);
            bindings?.Bind(channel);

            var consumer = new Consumer<TMessage>(Connect(), subscription, _provider);
            _consumers.Add(consumer);
            return new UnSubscriber(() =>
            {
                consumer.Dispose();
                _consumers.Remove(consumer);
            });
        }

        ~Subscriber()
        {
            Dispose(false);
        }

        private IConnection Connect()
        {
            if (!(_connection is null)) return _connection;

            lock (_sync)
            {
                return _connection ??= _connectionFactory.CreateConnection();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                foreach (var consumer in _consumers) consumer.Dispose();

                _connection?.Close();
                _connection?.Dispose();
            }

            _disposed = true;
        }
    }
}