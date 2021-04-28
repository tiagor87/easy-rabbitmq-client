using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Publisher.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Publisher.Tests.Models
{
    public class PublisherMessageTests
    {
        [Fact]
        public void GivenMessage_ShouldInstantiate()
        {
            const string body = "";
            const string correlationId = "correlationId";
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();

            var message = new PublisherMessage(publisherMock.Object, body, serializerMock.Object, routingMock.Object,
                correlationId,
                cancellationToken);

            message.Should().NotBeNull();
            message.Routing.Should().Be(routingMock.Object);
            message.CancellationToken.Should().Be(cancellationToken);
            message.CorrelationId.Should().Be(correlationId);
            message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GivenMessage_WhenInstantiate_ShouldAddDefaultHeader()
        {
            const string body = "";
            const string correlationId = "correlationId";
            const string exchange = "exchange";
            const string routingKey = "routingKey";
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            routingMock.SetupGet(x => x.ExchangeName)
                .Returns(exchange)
                .Verifiable();
            routingMock.SetupGet(x => x.RoutingKey)
                .Returns(routingKey)
                .Verifiable();
            var cancellationToken = new CancellationToken();

            var message = new PublisherMessage(publisherMock.Object, body, serializerMock.Object, routingMock.Object,
                correlationId,
                cancellationToken);

            var headers = message.GetHeaders();
            headers.Should().NotBeNull();
            headers.Should().Contain(nameof(IRouting.ExchangeName), exchange);
            headers.Should().Contain(nameof(IRouting.RoutingKey), routingKey);
            headers.Should().Contain("SerializerType", serializerMock.Object.GetType().FullName);
            headers.Should().ContainKey(nameof(message.CreatedAt));
        }

        [Fact]
        public void GivenMessage_ShouldAddHeader()
        {
            const string body = "";
            const string correlationId = "correlationId";
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();

            var message = new PublisherMessage(publisherMock.Object, body, serializerMock.Object, routingMock.Object,
                correlationId,
                cancellationToken);
            message.AddHeader("NewHeader", "NewHeaderValue");

            var headers = message.GetHeaders();
            headers.Should().NotBeNull();
            headers.Should().Contain("NewHeader", "NewHeaderValue");
        }

        [Fact]
        public void GivenMessage_WhenAddRepeatedHeader_ShouldUpdate()
        {
            const string body = "";
            const string correlationId = "correlationId";
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();

            var message = new PublisherMessage(publisherMock.Object, body, serializerMock.Object, routingMock.Object,
                correlationId,
                cancellationToken);
            message.AddHeader("NewHeader", "NewHeaderValue");
            message.AddHeader("NewHeader", "NewHeaderValue2");

            var headers = message.GetHeaders();
            headers.Should().NotBeNull();
            headers.Should().Contain("NewHeader", "NewHeaderValue2");
        }

        [Fact]
        public void GivenMessage_WhenSerialize_ShouldSerialize()
        {
            const string body = "test_message";
            const string correlationId = "correlationId";
            var resultBody = Encoding.UTF8.GetBytes(body);
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();
            serializerMock.Setup(x => x.Serialize(body))
                .Returns(resultBody)
                .Verifiable();

            var message = new PublisherMessage(publisherMock.Object, body, serializerMock.Object, routingMock.Object,
                correlationId,
                cancellationToken);

            var result = message.Serialize();

            message.Should().NotBeNull();
            Encoding.UTF8.GetString(result.ToArray()).Should().Be(body);
            serializerMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenPublish_ShouldPublish()
        {
            const string body = "test_message";
            const string correlationId = "correlationId";
            var publisherMock = new Mock<IPublisher>();
            var serializerMock = new Mock<IPublisherSerializer>();
            var routingMock = new Mock<IRouting>();
            var cancellationToken = new CancellationToken();
            publisherMock
                .Setup(x => x.PublishAsync(It.IsAny<PublisherMessage>(), cancellationToken))
                .Verifiable();

            var message = new PublisherMessage(
                publisherMock.Object,
                body,
                serializerMock.Object,
                routingMock.Object,
                correlationId,
                cancellationToken);

            await message.PublishAsync(cancellationToken);

            message.Should().NotBeNull();
            message.GetHeaders().Should().ContainKey("PublishedAt");
            message.GetHeaders()["PublishedAt"].ToString().Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
            publisherMock.VerifyAll();
        }
    }
}