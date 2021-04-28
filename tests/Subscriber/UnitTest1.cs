using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Shared.Models;
using EasyRabbitMqClient.Abstractions.Subscribers;
using EasyRabbitMqClient.Abstractions.Subscribers.Models;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Subscriber.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace EasyRabbitMqClient.Subscriber.Tests
{
    public class UnitTest1
    {
        [Fact(Skip = "Teste")]
        public void Test1()
        {
            var services = new ServiceCollection();
            services.AddScoped<ISubscriberSerializer, Serializer>();
            services.AddScoped<IBehavior<ISubscriberMessage>, LoggerBehavior>();
            services.AddScoped<ISubscriberHandler<Body>, SubscriberHandler>();
            var provider = services.BuildServiceProvider();
            var subscriber = new Subscriber(provider,
                new RabbitMqConnectionString("amqp://guest:guest@localhost/").CreateFactory());

            subscriber.Subscribe<SubscriberHandler, Body>();

            while (SubscriberHandler.Processed < 2 && LoggerBehavior.Executed < 2) Task.Delay(100).Wait();
        }
    }

    public class Body
    {
        public string Message { get; set; }
    }

    public class Serializer : ISubscriberSerializer
    {
        public T Deserialize<T>(ReadOnlyMemory<byte> body)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body.Span.ToArray()));
        }
    }

    public class LoggerBehavior : IBehavior<ISubscriberMessage>
    {
        public static int Executed { get; set; }

        public async Task ExecuteAsync(ISubscriberMessage message,
            Func<ISubscriberMessage, CancellationToken, Task> next, CancellationToken cancellationToken)
        {
            Executed++;
            await next(message, cancellationToken);
        }

        public void Dispose()
        {
        }
    }

    [Subscription("Test-All", 10)]
    [Exchange("test.topic", ExchangeType.Topic)]
    [Queue("Test-All")]
    [Binding("test.topic", "Test-All", "test.1.#")]
    [Binding("test.topic", "Test-All", "test.2.#")]
    public class SubscriberHandler : ISubscriberHandler<Body>
    {
        public static int Processed { get; set; }

        public Task HandleAsync(Body message, CancellationToken cancellationToken)
        {
            Processed++;
            return Task.CompletedTask;
        }
    }
}