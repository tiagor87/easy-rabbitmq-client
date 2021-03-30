namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IBufferPublisherBuilder<T> : IBufferBuilder<T>
    {
        IMessagePublisherBuilder<T> BuildAndReturn();
    }
}