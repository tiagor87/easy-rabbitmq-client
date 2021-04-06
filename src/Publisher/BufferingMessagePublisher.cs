using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.RetryBehaviors;
using EasyRabbitMqClient.Core.Models;
using RabbitMQ.Client;
using TRBufferList.Core;

namespace EasyRabbitMqClient.Publisher
{
    public class BufferingMessagePublisher : MessagePublisher
    {
        private readonly BufferList<IMessage> _buffer;

        public BufferingMessagePublisher(IConnectionFactory connectionFactory, IRetryBehavior retryBehavior, BufferList<IMessage> bufferList) : base(connectionFactory, retryBehavior)
        {
            _buffer = bufferList;
            _buffer.Cleared += OnBufferOnCleared;
        }

        public override async Task PublishAsync(IMessage message, CancellationToken cancellationToken)
        {
            await PublishBatchingAsync(new MessageBatching(new[] {message}), cancellationToken);
        }

        public override async Task PublishBatchingAsync(IMessageBatching messageBatch, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                foreach (var message in messageBatch)
                {
                    _buffer.Add(message);
                }
            }, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;
            
            if (disposing)
            {
                _buffer.Dispose();
            }
            
            base.Dispose(disposing);
        }

        private void OnBufferOnCleared(IReadOnlyList<IMessage> items)
        {
            var batching = new MessageBatching(items);
            base.PublishBatchingAsync(batching, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}