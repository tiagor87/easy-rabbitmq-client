using System.Collections.Generic;

namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IMessagePublisherBuilder<T>
    {
        IExchangePublisherBuilder<T> WithExchange();
        IMessagePublisherBuilder<T> WithExchange(IExchange exchange);
        IMessagePublisherBuilder<T> WithRoutingKey(string routingKey);
        IMessagePublisherBuilder<T> WithSerializer(IPublisherSerializer serializer);
        IBufferPublisherBuilder<T> WithBuffer();
        IMessagePublisherBuilder<T> WithBuffer(ICollection<T> buffer);
        IMessagePublisher<T> Build();
    }
}