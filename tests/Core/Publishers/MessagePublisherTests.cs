using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Extensions;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Core.Tests.Fixtures;
using EasyRabbitMqClient.Core.Tests.Serializers;
using EasyRabbitMqClient.Publisher.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Core.Tests.Publishers
{
    public class MessagePublisherTests : IClassFixture<PublisherFixture>
    {
        private readonly PublisherFixture _publisherFixture;

        public MessagePublisherTests(PublisherFixture publisherFixture)
        {
            _publisherFixture = publisherFixture;
        }
        
        [Fact]
        public async Task GivenMessageWhenPublishShouldAddHeaders()
        {
            var key = $"{nameof(GivenMessageWhenPublishShouldAddHeaders)}.{Guid.NewGuid()}";
            _publisherFixture.DeclareDirectExchange(key);
            _publisherFixture.DeclareQueueWithDirectBind(key, key);
            _publisherFixture.PurgeQueue(key);
            
            var message = new Message(key, new JsonSerializer(), new Routing(key, key), CancellationToken.None);
            await _publisherFixture.SyncPublisher.PublishAsync(message, CancellationToken.None);
            
            var resultMessage = _publisherFixture.GetMessage(key);

            resultMessage.BasicProperties.IsHeadersPresent().Should().BeTrue();
            resultMessage.BasicProperties.Headers.ContainsKey("CreatedAt").Should().BeTrue();
            resultMessage.BasicProperties.Headers["CreatedAt"].AsString().Should().Be(message.CreatedAt.ToString("s"));
            resultMessage.BasicProperties.Headers.ContainsKey("ExchangeName").Should().BeTrue();
            resultMessage.BasicProperties.Headers["ExchangeName"].AsString().Should().Be(message.Routing.ExchangeName);
            resultMessage.BasicProperties.Headers.ContainsKey("RoutingKey").Should().BeTrue();
            resultMessage.BasicProperties.Headers["RoutingKey"].AsString().Should().Be(message.Routing.RoutingKey);
            resultMessage.BasicProperties.Headers.ContainsKey("SerializerType").Should().BeTrue();
            resultMessage.BasicProperties.Headers["SerializerType"].AsString().Should().Be(message.Serializer.GetType().FullName);
            
            _publisherFixture.PurgeQueue(key);
        }

        [Fact]
        public async Task GivenMessageWhenPublishFailsShouldRetry()
        {
            var key = $"{nameof(GivenMessageWhenPublishFailsShouldRetry)}.{Guid.NewGuid()}";
            _publisherFixture.DeclareDirectExchange(key);
            _publisherFixture.DeclareQueueWithDirectBind(key, key);
            _publisherFixture.PurgeQueue(key);

            var jsonSerializerMock = new Mock<IPublisherSerializer>();
            jsonSerializerMock.SetupSequence(x => x.Serialize(key))
                .Throws<InvalidOperationException>()
                .Returns(Encoding.UTF8.GetBytes(key));
            var message = new Message(key, jsonSerializerMock.Object, new Routing(key, key), CancellationToken.None);
            await _publisherFixture.SyncPublisher.PublishAsync(message, CancellationToken.None);
            
            var resultMessage = _publisherFixture.GetMessage(key);
            resultMessage.Body.AsString().Should().Be(key);
            _publisherFixture.PurgeQueue(key);
            
            jsonSerializerMock.Verify(x => x.Serialize(key), Times.Exactly(2));
        }
        
        [Fact]
        public async Task GivenMessageWhenPublishFailsShouldNotifyError()
        {
            var key = $"{nameof(GivenMessageWhenPublishFailsShouldNotifyError)}.{Guid.NewGuid()}";
            _publisherFixture.DeclareDirectExchange(key);
            _publisherFixture.DeclareQueueWithDirectBind(key, key);
            _publisherFixture.PurgeQueue(key);

            var observerMock = new Mock<IObserver<IMessageBatching>>();
            observerMock.Setup(x => x.OnError(It.IsAny<PublishingException>()))
                .Verifiable();
            using var subscription = _publisherFixture.SyncPublisher.Subscribe(observerMock.Object);
            var jsonSerializerMock = new Mock<IPublisherSerializer>();
            jsonSerializerMock.Setup(x => x.Serialize(key))
                .Throws<InvalidOperationException>()
                .Verifiable();
            var message = new Message(key, jsonSerializerMock.Object, new Routing(key, key), CancellationToken.None);
            await _publisherFixture.SyncPublisher.PublishAsync(message, CancellationToken.None);
            
            _publisherFixture.PurgeQueue(key);
            jsonSerializerMock.VerifyAll();
            observerMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessageWhenPublishShouldNotifySuccess()
        {
            var key = $"{nameof(GivenMessageWhenPublishShouldNotifySuccess)}.{Guid.NewGuid()}";
            _publisherFixture.DeclareDirectExchange(key);
            _publisherFixture.DeclareQueueWithDirectBind(key, key);
            _publisherFixture.PurgeQueue(key);

            var observerMock = new Mock<IObserver<IMessageBatching>>();
            observerMock.Setup(x => x.OnNext(It.IsAny<IMessageBatching>()))
                .Verifiable();
            using var subscription = _publisherFixture.SyncPublisher.Subscribe(observerMock.Object);
            var jsonSerializerMock = new Mock<IPublisherSerializer>();
            jsonSerializerMock.Setup(x => x.Serialize(key))
                .Returns(Encoding.UTF8.GetBytes(key))
                .Verifiable();
            var message = new Message(key, jsonSerializerMock.Object, new Routing(key, key), CancellationToken.None);
            await _publisherFixture.SyncPublisher.PublishAsync(message, CancellationToken.None);
            
            _publisherFixture.PurgeQueue(key);
            jsonSerializerMock.VerifyAll();
            observerMock.VerifyAll();
        }
    }
}
