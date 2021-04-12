using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Exceptions;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Extensions;
using EasyRabbitMqClient.Publisher.Observers;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher
{
    public sealed class MessagePublisher : IMessagePublisher
    {
        private volatile bool _disposed;
        private IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IBehavior _behavior;
        private readonly HashSet<IObserver<IMessageBatching>> _observers;
        private static object _sync = new();

        public MessagePublisher(IConnectionFactory connectionFactory, IBehavior behavior)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _behavior = behavior;
            _observers = new HashSet<IObserver<IMessageBatching>>();
        }

        ~MessagePublisher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task PublishAsync(IMessage message, CancellationToken cancellationToken)
        {
            var batching = new MessageBatching(new [] { message });
            await PublishBatchingAsync(batching, cancellationToken);
        }

        public async Task PublishBatchingAsync(IMessageBatching messageBatch, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() => PublishBatching(messageBatch, cancellationToken), cancellationToken);
        }
        
        public IDisposable Subscribe(IObserver<IMessageBatching> observer)
        {
            _observers.Add(observer);
            return new UnSubscriber(() => _observers.Remove(observer));
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _connection?.Dispose();
                OnCompleted();
            }

            _disposed = true;
        }

        private IConnection Connect()
        {
            if (_connection is not null)
            {
                return _connection;
            }

            lock (_sync)
            {
                return _connection ??= _connectionFactory.CreateConnection();
            }
        }

        private void PublishBatching(IMessageBatching batching, CancellationToken cancellationToken =  default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MessagePublisher));

            bool Execute()
            {
                if (TryPublishMessages(batching, out var failed, cancellationToken)) return true;
                
                batching = failed;
                return false;
            }
            
            try
            {
                var result = _behavior?.Execute(Execute) ?? Execute();

                if (!result)
                {
                    OnError(new PublishingException(batching, "A few messages was not published.", null));
                }
            }
            catch (EasyRabbitMqClientException ex)
            {
                OnError(new PublishingException(batching, ex.Message, ex));
            }
            catch (Exception ex)
            {
                OnError(new PublishingException(batching, ex));
            }
        }
        
        private bool TryPublishMessages(IMessageBatching batching, out IMessageBatching failedBatching, CancellationToken cancellationToken)
        {
            const string lastException = "LastException";
            
            var hasMessageToSend = false;
            var failedMessages = new HashSet<IMessage>();
            failedBatching = null;
            using var model = Connect().CreateModel();
            try
            {
                model.ConfirmSelect();

                var batch = model.CreateBasicPublishBatch();
                foreach (var message in batching)
                {
                    try
                    {
                        hasMessageToSend = batch.Add(model, message) || hasMessageToSend;
                    }
                    catch (Exception ex)
                    {
                        message.AddHeader(lastException, ex.ToString());
                        failedMessages.Add(message);
                    }
                }
                    
                if (batching.Count == failedMessages.Count)
                {
                    failedBatching = new MessageBatching(failedMessages, batching.PublishingTimeout);
                    return false;
                }

                if (cancellationToken.IsCancellationRequested
                    || !hasMessageToSend) return true;

                batch.Publish();
                model.WaitForConfirmsOrDie(batching.PublishingTimeout);
                    
                if (!failedMessages.Any())
                {
                    OnNext(batching);
                    return true;
                }

                OnNext(new MessageBatching(batching.Except(failedMessages)));
                failedBatching = new MessageBatching(failedMessages, batching.PublishingTimeout);
                return false;
            }
            finally
            {
                model.Close();
            }
        }
        
        private void OnNext(IMessageBatching batching)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(batching);
            }
        }

        private void OnError(PublishingException exception)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(exception);
            }
        }

        private void OnCompleted()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }
    }
}