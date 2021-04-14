using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Core.Builders;
using EasyRabbitMqClient.Core.Exceptions;
using EasyRabbitMqClient.Core.Models;
using EasyRabbitMqClient.Publisher.Behaviors;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Tests.Fixtures;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace EasyRabbitMqClient.Publisher.Tests.Behaviors
{
    public class RabbitMqPublisherTests : IClassFixture<ConnectionFixture>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly Mock<IModel> _channelMock;
        private readonly Mock<IPublisherBehavior> _behaviorMock;
        private readonly Mock<IConnection> _connectionMock;

        public RabbitMqPublisherTests(ConnectionFixture connectionFixture)
        {
            _behaviorMock = new Mock<IPublisherBehavior>();
            _messagePublisher = new MessagePublisher(new RabbitMqPublisher(connectionFixture.GetConnectionFactory(out _connectionMock, out _channelMock)), _behaviorMock.Object);
        }

        private void SetupProxyPassBehavior()
        {
            _behaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IMessageBatching>(),
                        It.IsAny<Func<IMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Returns((IMessageBatching batching, Func<IMessageBatching, CancellationToken, Task> action, CancellationToken cancellationToken) => action(batching, cancellationToken))
                .Verifiable();
        }
        
        [Fact]
        public async Task GivenMessage_ShouldNotCreateConnectionTwice()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();
            SetupProxyPassBehavior();
            
            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()))
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
            
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
            _connectionMock.Verify(x => x.CreateModel(), Times.Exactly(2));
        }

        [Fact]
        public async Task GivenMessage_ShouldPublishAndConfirm()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();
            SetupProxyPassBehavior();
            
            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()))
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
            
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Fact]
        public void GivenMessage_WhenCreateNewMessage_ShouldReturnBuilder()
        {
            var messageBuilder = _messagePublisher.NewMessage();

            messageBuilder.Publisher.Should().Be(_messagePublisher);
            messageBuilder.Should().NotBeNull();
            messageBuilder.Should().BeOfType<MessageBuilder>();
        }
        
        [Fact]
        public async Task GivenMessage_WhenPublishMultipleMessagesAndHasFailure_ShouldNotify()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            const string correlationId = "correlationId";
            var headersMock = new Mock<IDictionary<string, object>>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            SetupProxyPassBehavior();
            
            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()))
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
            messageMock.SetupSequence(x => x.Equals(It.IsAny<IMessage>()))
                .Returns(false)
                .Returns(true)
                .Returns(false);
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

            using var _ = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishBatchingAsync(new MessageBatching(new [] { messageMock.Object, messageMock.Object, messageMock.Object }), CancellationToken.None);
            
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
            observerMock.Verify(x => x.OnNext(It.Is<IMessageBatching>(y => y.Count == 2)));
            observerMock.Verify(x => x.OnError(It.Is<PublishingException>(y => y.Batching.Count == 1)));
        }
        
        [Fact]
        public async Task GivenMessage_WhenPublish_ShouldNotifyObservers()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            
            var routingMock = new Mock<IRouting>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            
            var observerMock = new Mock<IObserver<IMessageBatching>>(); 
            var headersMock = new Mock<IDictionary<string, object>>();
            
            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.CreateBasicProperties())
                .Returns(basicPropertiesMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();
            _channelMock.Setup(x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()))
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
            
            observerMock.Setup(x => x.OnNext(It.IsAny<IMessageBatching>()))
                .Verifiable();
            
            SetupProxyPassBehavior();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.VerifyAll();
            unsubscribe.Should().NotBeNull();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            observerMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessage_WhenFail_ShouldSetExceptionAndNotPublish()
        {
            var batchMock = new Mock<IBasicPublishBatch>();
            var messageMock = new Mock<IMessage>();
            
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
            
            SetupProxyPassBehavior();
            
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            
            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessage_WhenFail_ShouldNotifyObservers()
        {
            var batchMock = new Mock<IBasicPublishBatch>();
            var messageMock = new Mock<IMessage>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
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
            
            observerMock.Setup(x => x.OnError(It.IsAny<PublishingException>()))
                .Verifiable();
            
            SetupProxyPassBehavior();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            unsubscribe.Should().NotBeNull();
            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
            observerMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Theory]
        [MemberData(nameof(GetExceptions))]
        public async Task GivenMessage_WhenFails_ShouldCreateExceptionWithMessageBatching(Exception exception)
        {
            var messageMock = new Mock<IMessage>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            _channelMock.Setup(x => x.ConfirmSelect())
                .Throws(exception)
                .Verifiable();
            
            observerMock.Setup(x => x.OnError(It.IsAny<PublishingException>()))
                .Callback((Exception ex) => ex.As<PublishingException>().Batching.Should().NotBeNull())
                .Verifiable();
            
            SetupProxyPassBehavior();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            unsubscribe.Should().NotBeNull();
            messageMock.VerifyAll();
            observerMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessage_WhenRabbitLibraryThrow_ShouldNotifyError()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
            var headersMock = new Mock<IDictionary<string, object>>();
            
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
            
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            observerMock.Setup(x => x.OnError(It.IsAny<Exception>()))
                .Verifiable();
            
            SetupProxyPassBehavior();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);

            unsubscribe.Should().NotBeNull();
            observerMock.VerifyAll();
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Fact]
        public void GivenMessage_WhenDispose_ShouldNotifyObservers()
        {
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            observerMock.Setup(x => x.OnCompleted())
                .Verifiable();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            _messagePublisher.Dispose();
            
            unsubscribe.Should().NotBeNull();
            observerMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessage_WhenDispose_ShouldNotAllowPublishing()
        {
            var messageMock = new Mock<IMessage>();
            
            _messagePublisher.Dispose();

            await _messagePublisher.Awaiting(x => x.PublishAsync(messageMock.Object, CancellationToken.None))
                .Should()
                .ThrowAsync<ObjectDisposedException>();
        }
        
        [Fact]
        public void GivenSubscriberWhenUnsubscribe_ShouldRemoveObserver()
        {
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            observerMock.Setup(x => x.OnCompleted())
                .Verifiable();

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            unsubscribe.Dispose();
            _messagePublisher.Dispose();
            
            unsubscribe.Should().NotBeNull();
            observerMock.Verify(x => x.OnCompleted(), Times.Never());
        }
        
        [Fact]
        public async Task GivenMessage_WhenCancellationRequested_ShouldSkipMessage()
        {
            var batchMock = new Mock<IBasicPublishBatch>();
            var messageMock = new Mock<IMessage>();
            
            _channelMock.Setup(x => x.CreateBasicPublishBatch())
                .Returns(batchMock.Object)
                .Verifiable();
            _channelMock.Setup(x => x.ConfirmSelect())
                .Verifiable();

            var cancellationTokenSource = new CancellationTokenSource();
            messageMock.SetupGet(x => x.CancellationToken)
                .Returns(cancellationTokenSource.Token)
                .Verifiable();
            
            SetupProxyPassBehavior();
            
            cancellationTokenSource.Cancel();
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            
            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        [Theory]
        [MemberData(nameof(GetExceptions))]
        public async Task GivenMessage_WhenFailedOnBehavior_ShouldCallObservers(Exception exception)
        {
            var messageMock = new Mock<IMessage>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            _behaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IMessageBatching>(),
                        It.IsAny<Func<IMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Throws(exception)
                .Verifiable();
            
            observerMock.Setup(x => x.OnError(It.IsAny<PublishingException>()))
                .Verifiable();
            
            using var subscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            observerMock.VerifyAll();
            _behaviorMock.VerifyAll();
        }
        
        public static IEnumerable<object[]> GetExceptions()
        {
            yield return new object[]
            {
                new Exception()
            };
            
            yield return new object[]
            {
                new ForbiddenException(string.Empty, new Exception())
            };
            
            yield return new object[]
            {
                new NotFoundException(string.Empty, new Exception())
            };
        }
    }
}