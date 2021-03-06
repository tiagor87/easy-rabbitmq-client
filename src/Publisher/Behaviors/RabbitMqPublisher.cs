using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Extensions;
using EasyRabbitMqClient.Publisher.Models;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Behaviors
{
    public class RabbitMqPublisher : IPublisherBehavior
    {
        private static readonly object _sync = new object();
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private bool _disposed;

        public RabbitMqPublisher(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task PublishAsync(IPublisherMessageBatching batching, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqPublisher));

            if (!batching.Any()) return;

            await Task.Factory.StartNew(() =>
            {
                const string lastException = "LastException";

                try
                {
                    var failedMessages = new HashSet<IPublisherMessage>(0);
                    var hasSuccessMessages = false;
                    using var model = Connect().CreateModel();
                    try
                    {
                        model.ConfirmSelect();

                        var batch = model.CreateBasicPublishBatch();
                        foreach (var message in batching)
                            try
                            {
                                hasSuccessMessages = batch.Add(model, message) || hasSuccessMessages;
                            }
                            catch (Exception ex)
                            {
                                message.AddHeader(lastException, ex.ToString());
                                failedMessages.Add(message);
                            }

                        if (batching.Count == failedMessages.Count)
                            throw new PublishingException(batching, "All messages failed.", null);

                        if (cancellationToken.IsCancellationRequested
                            || !hasSuccessMessages) return Task.CompletedTask;

                        batch.Publish();
                        if (!model.WaitForConfirms(batching.PublishingTimeout))
                            throw new PublishingNotConfirmedException(
                                new PublisherMessageBatching(batching.Publisher, batching));

                        if (!failedMessages.Any()) return Task.CompletedTask;

                        throw new PublishingException(new PublisherMessageBatching(batching.Publisher, failedMessages),
                            "A few messages failed.",
                            null);
                    }
                    finally
                    {
                        model.Close();
                    }
                }
                catch (PublishingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new PublishingException(batching, ex);
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RabbitMqPublisher()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing) _connection?.Dispose();

            _disposed = true;
        }

        private IConnection Connect()
        {
            if (!(_connection is null)) return _connection;

            lock (_sync)
            {
                return _connection ??= _connectionFactory.CreateConnection();
            }
        }
    }
}