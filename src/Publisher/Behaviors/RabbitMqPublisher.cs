using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Extensions;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Behaviors
{
    public class RabbitMqPublisher : PublisherBehaviorBase
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private static object _sync = new();

        public RabbitMqPublisher(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        
        protected override Task PublishAsync(IMessageBatching batching, CancellationToken cancellationToken)
        {
            const string lastException = "LastException";
            
            var failedMessages = new HashSet<IMessage>(0);
            var hasSuccessMessages = false;
            using var model = Connect().CreateModel();
            try
            {
                model.ConfirmSelect();

                var batch = model.CreateBasicPublishBatch();
                foreach (var message in batching)
                {
                    try
                    {
                        hasSuccessMessages = batch.Add(model, message) || hasSuccessMessages;
                    }
                    catch (Exception ex)
                    {
                        message.AddHeader(lastException, ex.ToString());
                        failedMessages.Add(message);
                    }
                }
                    
                if (batching.Count == failedMessages.Count)
                {
                    throw new PublishingException(batching, "All messages failed.", null);
                }

                if (cancellationToken.IsCancellationRequested
                    || !hasSuccessMessages) return Task.CompletedTask;

                batch.Publish();
                model.WaitForConfirmsOrDie(batching.PublishingTimeout);
                    
                if (!failedMessages.Any())
                {
                    return Task.CompletedTask;
                }

                throw new PublishingException(new MessageBatching(failedMessages), "A few messages failed.", null);
            }
            finally
            {
                model.Close();
            }
        }

        protected override void OnDispose()
        {
            _connection?.Dispose();
        }
        
        private IConnection Connect()
        {
            if (_connection is not null)
            {
                return _connection;
            }

            lock (_sync)
            {
                return _connection ??= _connectionFactory.CreateConnection();
            }
        }
    }
}