using System;
using EasyRabbitMqClient.Abstractions.Builders;
using EasyRabbitMqClient.Core.Builders;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core
{
    public class EasyRabbitMqClient
    {
        private readonly IAsyncConnectionFactory _connectionFactory;

        public EasyRabbitMqClient()
        {
            _connectionFactory = CreateConnectionFactory("amqp://guest:guest@localhost:5671/");
        }

        public IMessagePublisherBuilder<T> CreatePublisher<T>()
        {
            return new MessagePublisherBuilder<T>(_connectionFactory);
        }

        private static IAsyncConnectionFactory CreateConnectionFactory(string connectionString)
        {
            return  new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }
    }
}
