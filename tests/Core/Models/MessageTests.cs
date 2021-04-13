using System;
using System.Text;
using System.Threading;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Core.Tests.Models
{
    public class MessageTests
    {
        [Fact]
        public void GivenMessage_ShouldInstantiate()
        {
            const string body = "";
            const string correlationId = "correlationId";
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();

            var message = new Message(body, serializerMock.Object, routingMock.Object, correlationId,
                cancellationToken);

            message.Should().NotBeNull();
            message.Routing.Should().Be(routingMock.Object);
            message.CancellationToken.Should().Be(cancellationToken);
            message.CorrelationId.Should().Be(correlationId);
            message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public void GivenMessage_WhenSerialize_ShouldSerialize()
        {
            const string body = "test_message";
            const string correlationId = "correlationId";
            var resultBody = Encoding.UTF8.GetBytes(body);
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();
            serializerMock.Setup(x => x.Serialize(body))
                .Returns(resultBody)
                .Verifiable();
            
            var message = new Message(body, serializerMock.Object, routingMock.Object, correlationId,
                cancellationToken);

            var result = message.Serialize();
            
            message.Should().NotBeNull();
            Encoding.UTF8.GetString(result.ToArray()).Should().Be(body);
            serializerMock.VerifyAll();
        }
    }
}