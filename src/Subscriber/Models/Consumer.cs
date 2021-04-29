using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Subscribers;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;
using EasyRabbitMqClient.Core.Behaviors;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Subscriber.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Subscriber.Models
{
    internal static class ChannelSyncState
    {
        public static volatile object Sync = new object();
    }

    internal class Consumer<TMessage> : AsyncDefaultBasicConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly ISubscription _options;
        private readonly IServiceProvider _provider;
        private readonly CancellationTokenSource _tokenSource;
        private IModel _channel;
        private string _consumerTag;
        private bool _disposed;

        internal Consumer(IConnection connection, ISubscription options, IServiceProvider provider)
        {
            _connection = connection;
            _options = options;
            _provider = provider;
            _tokenSource = new CancellationTokenSource();
            TrySubscribeAsync(_tokenSource.Token);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
            string exchange, string routingKey,
            IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var serializer = scope.ServiceProvider.GetRequiredService<ISubscriberSerializer>();

            var message = new SubscriberMessage(
                _channel,
                serializer,
                deliveryTag,
                new Routing(exchange, routingKey),
                properties,
                body);

            try
            {
                var handler = scope.ServiceProvider.GetRequiredService<ISubscriberHandler<TMessage>>();
                var behaviors = scope.ServiceProvider.GetServices<IBehavior<ISubscriberMessage>>().ToList();
                var behavior =
                    AggregateBehavior<ISubscriberMessage>.Create(new SubscriberBehaviorWrapper<TMessage>(handler),
                        behaviors);
                await behavior.ExecuteAsync(message, null, _tokenSource.Token);

                if (!_options.AutoAck) return;

                await message.AckAsync(_tokenSource.Token);
            }
            catch
            {
                await message.NotAckAsync(_tokenSource.Token);
            }
        }

        ~Consumer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed) _tokenSource.Cancel();

            _disposed = true;
        }

        private bool TrySubscribe(CancellationToken cancellationToken)
        {
            TryDisposeChannel();

            try
            {
                lock (ChannelSyncState.Sync)
                {
                    if (cancellationToken.IsCancellationRequested || _disposed) return true;
                    _channel = _connection.CreateModel();
                    _channel.BasicQos(0, _options.PrefetchCount, false);
                    _consumerTag = _channel.BasicConsume(_options.QueueName, false, this);
                    _channel.ModelShutdown += OnChannelShutdown;
                    Model = _channel;
                }

                return true;
            }
            catch
            {
                TryDisposeChannel();
                return false;
            }
        }

        private void OnChannelShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application) return;
            TrySubscribeAsync(_tokenSource.Token);
        }

        private void TrySubscribeAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested && !TrySubscribe(cancellationToken))
                    Task.Delay(_options.ReconnectDelayInMs, cancellationToken).ConfigureAwait(false).GetAwaiter()
                        .GetResult();
            }, TaskCreationOptions.LongRunning);
        }

        private void TryDisposeChannel()
        {
            if (_channel == null) return;

            lock (ChannelSyncState.Sync)
            {
                if (_channel.IsOpen)
                {
                    _channel.BasicCancel(_consumerTag ?? string.Empty);
                    _channel.Close();
                }

                _channel.Dispose();
                _channel = null;
            }
        }
    }
}