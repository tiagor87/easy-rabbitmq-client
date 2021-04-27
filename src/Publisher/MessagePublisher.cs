using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Behaviors;
using EasyRabbitMqClient.Core.Builders;
using EasyRabbitMqClient.Core.Exceptions;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Publisher.Behaviors;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Observers;

namespace EasyRabbitMqClient.Publisher
{
    public sealed class MessagePublisher : IMessagePublisher
    {
        private volatile bool _disposed;
        private readonly IBehavior _behavior;
        private readonly HashSet<IObserver<IMessageBatching>> _observers;

        public MessagePublisher(IPublisherBehavior publisher, params IBehavior[] behaviors)
        {
            _behavior = AggregateBehavior.Create(new PublisherBehaviorWrapper(publisher), behaviors);
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

        public IMessageBuilder<IPublishingMessage> NewMessage()
        {
            return new MessageBuilder()
                .ForPublisher(this);
        }

        public async Task PublishAsync(IMessage message, CancellationToken cancellationToken)
        {
            var batching = new MessageBatching(new [] { message });
            await PublishBatchingAsync(batching, cancellationToken);
        }
        
        public async Task PublishBatchingAsync(IMessageBatching batching, CancellationToken cancellationToken =  default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MessagePublisher));

            try
            {
                await _behavior.ExecuteAsync(batching, null, cancellationToken);
                OnNext(batching);
            }
            catch (PublishingException ex)
            {
                OnError(ex);
                var success = new MessageBatching(batching.Except(ex.Batching));
                if (success.Any()) OnNext(success);
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
        
        public IDisposable Subscribe(IObserver<IMessageBatching> observer)
        {
            _observers.Add(observer);
            return new UnSubscriber(() => _observers.Remove(observer));
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _behavior.Dispose();
                OnCompleted();
            }

            _disposed = true;
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