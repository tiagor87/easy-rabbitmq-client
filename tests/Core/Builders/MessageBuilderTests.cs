using System;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Builders;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Core.Tests.Builders
{
    public class MessageBuilderTests
    {
        [Fact]
        public void GivenBuilder_ShouldBuild()
        {
            const string message = "";
            const string exchange = "exchange";
            const string routingKey = "routingKey";
            const string correlationId = "correlationId";
            var publisherMock = new Mock<IMessagePublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            
            var publishingMessage = new MessageBuilder()
                .ForPublisher(publisherMock.Object)
                .WithMessage(message)
                .WithRouting(exchange, routingKey)
                .WithSerializer(serializerMock.Object)
                .WithCancellationToken(CancellationToken.None)
                .WithCorrelationId(correlationId)
                .Build();

            publishingMessage.Should().NotBeNull();
            publishingMessage.CorrelationId.Should().Be(correlationId);
            publishingMessage.Routing.Should().NotBeNull();
            publishingMessage.Routing.ExchangeName.Should().Be(exchange);
            publishingMessage.Routing.RoutingKey.Should().Be(routingKey);
            publishingMessage.CancellationToken.Should().NotBeNull();
            publishingMessage.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            serializerMock.VerifyAll();
        }
    }
}