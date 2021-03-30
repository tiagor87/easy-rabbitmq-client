using EasyRabbitMqClient.Abstractions.Builders;

namespace EasyRabbitMqClient.Core.Builders
{
    public class BufferPublisherBuilder<T> : BufferBuilder<T>, IBufferPublisherBuilder<T>
    {
        private readonly IMessagePublisherBuilder<T> _publisherBuilder;

        public BufferPublisherBuilder(IMessagePublisherBuilder<T> publisherBuilder)
        {
            _publisherBuilder = publisherBuilder;
        }
        
        public IMessagePublisherBuilder<T> BuildAndReturn()
        {
            var buffer = Build();
            return _publisherBuilder.WithBuffer(buffer);
        }
    }
}