using EasyRabbitMqClient.Abstractions.Publishers.Models;

namespace EasyRabbitMqClient.Publisher.Exceptions
{
    public class PublishingNotConfirmedException : PublishingException
    {
        public PublishingNotConfirmedException(IPublisherMessageBatching batching)
            : base(batching, "The publishing was not confirmed.", null)
        {
        }
    }
}