using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Builders;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Behaviors;
using EasyRabbitMqClient.Core.Exceptions;
using EasyRabbitMqClient.Core.Observers;
using EasyRabbitMqClient.Publisher.Behaviors;
using EasyRabbitMqClient.Publisher.Builders;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Models;

namespace EasyRabbitMqClient.Publisher
{
    public sealed class Publisher : IPublisher
    {
        private readonly IBehavior<IPublisherMessageBatching> _behavior;
        private readonly HashSet<IObserver<IPublisherMessageBatching>> _observers;
        private volatile bool _disposed;

        public Publisher(IPublisherBehavior publisher, params IBehavior<IPublisherMessageBatching>[] behaviors)
        {
            _behavior = AggregateBehavior<IPublisherMessageBatching>.Create(new PublisherBehaviorWrapper(publisher),
                behaviors);
            _observers = new HashSet<IObserver<IPublisherMessageBatching>>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IPublisherMessageBuilder<IPublisherMessage> NewMessage()
        {
            return new PublisherMessageBuilder()
                .ForPublisher(this);
        }

        public async Task PublishAsync(IPublisherMessage publisherMessage, CancellationToken cancellationToken)
        {
            var batching = new PublisherMessageBatching(new[] {publisherMessage});
            await PublishAsync(batching, cancellationToken);
        }

        public async Task PublishAsync(IPublisherMessageBatching batching,
            CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Publisher));

            try
            {
                await _behavior.ExecuteAsync(batching, null, cancellationToken);
                OnNext(batching);
            }
            catch (PublishingException ex)
            {
                OnError(ex);
                var success = new PublisherMessageBatching(batching.Except(ex.Batching));
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

        public IDisposable Subscribe(IObserver<IPublisherMessageBatching> observer)
        {
            _observers.Add(observer);
            return new UnSubscriber(() => _observers.Remove(observer));
        }

        ~Publisher()
        {
            Dispose(false);
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

        private void OnNext(IPublisherMessageBatching batching)
        {
            foreach (var observer in _observers) observer.OnNext(batching);
        }

        private void OnError(PublishingException exception)
        {
            foreach (var observer in _observers) observer.OnError(exception);
        }

        private void OnCompleted()
        {
            foreach (var observer in _observers) observer.OnCompleted();
        }
    }
}