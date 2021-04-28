using System;
using System.Security.Authentication;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Core.Models
{
    public class RabbitMqConnectionString
    {
        public RabbitMqConnectionString(string connectionString)
        {
            Value = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string Value { get; }

        public static implicit operator RabbitMqConnectionString(string connectionString)
        {
            return new RabbitMqConnectionString(connectionString);
        }

        public static implicit operator string(RabbitMqConnectionString connectionString)
        {
            return connectionString?.Value;
        }

        public IConnectionFactory CreateFactory()
        {
            const string sslSchema = "amqps";
            const int recoveryInterval = 5;
            const int requestedHeartbeat = 5;
            const int consumerDispatchConcurrency = 100;

            var uri = new Uri(Value);
            return new ConnectionFactory
            {
                Uri = uri,
                TopologyRecoveryEnabled = true,
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true,
                UseBackgroundThreadsForIO = true,

                ConsumerDispatchConcurrency = consumerDispatchConcurrency,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(recoveryInterval),
                RequestedHeartbeat = TimeSpan.FromSeconds(requestedHeartbeat),

                Ssl = new SslOption
                {
                    Enabled = uri.Scheme.Equals(sslSchema, StringComparison.InvariantCultureIgnoreCase),
                    ServerName = uri.Host,
                    Version = SslProtocols.None | SslProtocols.Tls12
                }
            };
        }
    }
}