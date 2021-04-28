using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Publisher.Behaviors;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Models;
using EasyRabbitMqClient.Publisher.Tests.Fixtures;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace EasyRabbitMqClient.Publisher.Tests.Behaviors
{
    public class RabbitMqPublisherTests : IClassFixture<ConnectionFixture>
    {
        private readonly Mock<IModel> _channelMock;
        private readonly Mock<IConnection> _connectionMock;
        private readonly IPublisherBehavior _publisher;

        public RabbitMqPublisherTests(ConnectionFixture connectionFixture)
        {
            _publisher =
                new RabbitMqPublisher(connectionFixture.GetConnectionFactory(out _connectionMock, out _channelMock));
        }

        [Fact]
        public async Task GivenMessage_ShouldNotCreateConnectionTwice()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IPublisherMessage>();
            var publisherMock = new Mock<IPublisher>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirms(It.IsAny<TimeSpan>()))
                .Returns(true)
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.Setup(x => x.Serialize())
                .Returns(bytes)
                .Verifiable();
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();
            messageMock.SetupGet(x => x.CorrelationId)
                .Returns(correlationId)
                .Verifiable();


            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.PublishAsync(batching, CancellationToken.None);
            await _publisher.PublishAsync(batching, CancellationToken.None);

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            _connectionMock.Verify(x => x.CreateModel(), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenMessage_ShouldPublishAndConfirm()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IPublisherMessage>();
            var publisherMock = new Mock<IPublisher>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirms(It.IsAny<TimeSpan>()))
                .Returns(true)
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.Setup(x => x.Serialize())
                .Returns(bytes)
                .Verifiable();
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();
            messageMock.SetupGet(x => x.CorrelationId)
                .Returns(correlationId)
                .Verifiable();


            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.PublishAsync(batching, CancellationToken.None);

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenPublishMultipleMessagesAndHasFailure_ShouldThrow()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IPublisherMessage>();
            var publisherMock = new Mock<IPublisher>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirms(It.IsAny<TimeSpan>()))
                .Returns(true)
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.SetupSequence(x => x.Serialize())
                .Returns(bytes)
                .Throws<Exception>()
                .Returns(bytes);
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();
            messageMock.SetupGet(x => x.CorrelationId)
                .Returns(correlationId)
                .Verifiable();

            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Verifiable();

            var batching = new PublisherMessageBatching(
                publisherMock.Object,
                messageMock.Object,
                messageMock.Object,
                messageMock.Object);

            await _publisher.Awaiting(x => x.PublishAsync(
                    batching,
                    CancellationToken.None))
                .Should()
                .ThrowAsync<PublishingException>();

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenPublish_ShouldReturn()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var publisherMock = new Mock<IPublisher>();

            var routingMock = new Mock<IRouting>();
            const string exchange = "exchange";
            const string routingKey = "exchange";

            var messageMock = new Mock<IPublisherMessage>();
            var bytes = Array.Empty<byte>();

            var headersMock = new Mock<IDictionary<string, object>>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirms(It.IsAny<TimeSpan>()))
                .Returns(true)
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.Setup(x => x.Serialize())
                .Returns(bytes)
                .Verifiable();
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();

            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.PublishAsync(batching, CancellationToken.None);

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenNotConfirm_ShouldThrow()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var publisherMock = new Mock<IPublisher>();

            var routingMock = new Mock<IRouting>();
            const string exchange = "exchange";
            const string routingKey = "exchange";

            var messageMock = new Mock<IPublisherMessage>();
            var bytes = Array.Empty<byte>();

            var headersMock = new Mock<IDictionary<string, object>>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirms(It.IsAny<TimeSpan>()))
                .Returns(false)
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.Setup(x => x.Serialize())
                .Returns(bytes)
                .Verifiable();
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();

            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.Awaiting(x => x.PublishAsync(batching, CancellationToken.None))
                .Should().ThrowAsync<PublishingNotConfirmedException>();

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenFail_ShouldThrowAndNotPublish()
        {
            var batchMock = new Mock<IBasicPublishBatch>();
            var messageMock = new Mock<IPublisherMessage>();
            var publisherMock = new Mock<IPublisher>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();

            messageMock.SetupGet(x => x.Routing)
                .Throws<Exception>()
                .Verifiable();
            messageMock.Setup(x => x.AddHeader("LastException", It.IsAny<string>()))
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.Awaiting(x => x.PublishAsync(batching, CancellationToken.None))
                .Should().ThrowAsync<PublishingException>();

            batchMock.Verify(x => x.Publish(), Times.Never());
            _channelMock.Verify(
                x => x.WaitForConfirms(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();

            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenRabbitLibraryThrow_ShouldThrow()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IPublisherMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            var headersMock = new Mock<IDictionary<string, object>>();
            var publisherMock = new Mock<IPublisher>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();

            routingMock.SetupGet(x => x.ExchangeName).Returns(exchange).Verifiable();
            routingMock.SetupGet(x => x.RoutingKey).Returns(routingKey).Verifiable();
            messageMock.SetupGet(x => x.Routing)
                .Returns(routingMock.Object)
                .Verifiable();
            messageMock.Setup(x => x.CancellationToken)
                .Returns(CancellationToken.None)
                .Verifiable();
            messageMock.Setup(x => x.Serialize())
                .Returns(bytes)
                .Verifiable();
            messageMock.Setup(x => x.GetHeaders())
                .Returns(headersMock.Object)
                .Verifiable();


            batchMock.Setup(x =>
#pragma warning disable 618
                    // New Add method is an extension and can not be overriden
                    x.Add(
#pragma warning restore 618
                        exchange,
                        routingKey,
                        false,
                        basicPropertiesMock.Object,
                        bytes))
                .Verifiable();
            batchMock.Setup(x => x.Publish())
                .Throws<Exception>()
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.Awaiting(x => x.PublishAsync(batching, CancellationToken.None))
                .Should().ThrowAsync<PublishingException>();

            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }

        [Fact]
        public async Task GivenMessage_WhenDispose_ShouldDisposeConnection()
        {
            await GivenMessage_WhenPublish_ShouldReturn();

            _publisher.Dispose();

            _connectionMock.Verify(x => x.Dispose(), Times.Once());
        }

        [Fact]
        public async Task GivenMessage_WhenDispose_ShouldNotAllowPublishing()
        {
            var batchingMock = new Mock<IPublisherMessageBatching>();

            _publisher.Dispose();

            await _publisher.Awaiting(x => x.PublishAsync(batchingMock.Object, CancellationToken.None))
                .Should()
                .ThrowAsync<ObjectDisposedException>();
        }

        [Fact]
        public async Task GivenMessage_WhenIsEmpty_ShouldReturn()
        {
            var publisher = new Mock<IPublisher>();
            var batching = new PublisherMessageBatching(publisher.Object);

            await _publisher.PublishAsync(batching, CancellationToken.None);

            _connectionMock.Verify(x => x.CreateModel(), Times.Never());
        }

        [Fact]
        public async Task GivenMessage_WhenCancellationRequested_ShouldSkipMessage()
        {
            var batchMock = new Mock<IBasicPublishBatch>();
            var messageMock = new Mock<IPublisherMessage>();
            var publisherMock = new Mock<IPublisher>();

            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();

            var cancellationTokenSource = new CancellationTokenSource();
            messageMock.SetupGet(x => x.CancellationToken)
                .Returns(cancellationTokenSource.Token)
                .Verifiable();

            cancellationTokenSource.Cancel();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await _publisher.PublishAsync(batching, CancellationToken.None);

            _channelMock.Verify(
                x => x.WaitForConfirms(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();

            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
        }
    }
}