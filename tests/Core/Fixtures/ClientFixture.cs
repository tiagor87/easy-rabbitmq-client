using System;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.RetryBehaviors;
using EasyRabbitMqClient.Publisher;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core.Tests.Fixtures
{
    public class PublisherFixture : IDisposable
    {
        public PublisherFixture()
        {
            var connectionFactory = CreateConnectionFactory();
            Connection = connectionFactory.CreateConnection();
            SyncPublisher = new MessagePublisher(
                connectionFactory,
                new ArithmeticRetryBehavior(
                    1,
                    TimeSpan.FromMilliseconds(100),
                    2,
                    TimeSpan.FromMilliseconds(150)));
        }

        public IMessagePublisher SyncPublisher { get; }

        public IConnection Connection { get; }

        private IConnectionFactory CreateConnectionFactory()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672/"),
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true
            };
            return factory;
        }
        
        public void Dispose()
        {
            SyncPublisher.Dispose();
        }

        public uint MessageCount(string queueName)
        {
            using var model = Connection.CreateModel();
            return model.MessageCount(queueName);
        }

        public void PurgeQueue(string queueName)
        {
            using var model = Connection.CreateModel();
            model.QueuePurge(queueName);
        }

        public void DeclareQueue(string queueName)
        {
            using var model = Connection.CreateModel();
            model.QueueDeclare(queueName, false, false);
        }

        public void DeclareDirectExchange(string exchangeName)
        {
            using var model = Connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, true);
        }

        public void DeclareQueueWithDirectBind(string exchangeName, string queueName)
        {
            using var model = Connection.CreateModel();
            DeclareQueue(queueName);
            model.QueueBind(queueName, exchangeName, queueName);
        }

        public BasicGetResult GetMessage(string queueName)
        {
            using var model = Connection.CreateModel();
            return model.BasicGet(queueName, true);
        }
    }
}