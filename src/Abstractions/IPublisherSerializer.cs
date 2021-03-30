namespace EasyRabbitMqClient.Abstractions
{
    public interface IPublisherSerializer
    {
        byte[] Serialize(object message);
    }
}