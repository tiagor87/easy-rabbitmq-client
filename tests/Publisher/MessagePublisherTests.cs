using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Models;
using EasyRabbitMqClient.Abstractions.Publishers;
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Tests.Fixtures;
using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace EasyRabbitMqClient.Publisher.Tests
{
    public class MessagePublisherTests : IClassFixture<ConnectionFixture>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly Mock<IModel> _channelMock;

        public MessagePublisherTests(ConnectionFixture connectionFixture)
        {
            _messagePublisher = new MessagePublisher(connectionFixture.GetConnectionFactory(out _channelMock), null);
        }

        [Fact]
        public async Task GivenMessageShouldPublishAndConfirm()
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
        }
        
        [Fact]
        public async Task GivenMessageWhenPublishShouldNotifyObservers()
        {
            var basicPropertiesMock = new Mock<IBasicProperties>();
            var batchMock = new Mock<IBasicPublishBatch>();
            var routingMock = new Mock<IRouting>();
            var messageMock = new Mock<IMessage>();
            var bytes = Array.Empty<byte>();
            const string exchange = "exchange";
            const string routingKey = "exchange";
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

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.VerifyAll();
            unsubscribe.Should().NotBeNull();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
            observerMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessageWhenFailShouldSetExceptionAndNotPublish()
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
            
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            
            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
        }
        
        [Fact]
        public async Task GivenMessageWhenFailShouldNotifyObservers()
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
        }
        
        [Fact]
        public async Task GivenMessageWhenFailShouldCreateExceptionWithMessages()
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
                .Callback((Exception ex) => ex.As<PublishingException>().Batching.Should().NotBeNull())
                .Verifiable();

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

        }
        
        [Fact]
        public async Task GivenMessageWhenRabbitLibraryThrowShouldNotifyError()
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

            var unsubscribe = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);

            unsubscribe.Should().NotBeNull();
            observerMock.VerifyAll();
            _channelMock.VerifyAll();
            messageMock.VerifyAll();
            batchMock.VerifyAll();
        }
        
        [Fact]
        public void GivenMessageWhenDisposeShouldNotifyObservers()
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
        public async Task GivenMessageWhenDisposeShouldNotAllowPublishing()
        {
            var messageMock = new Mock<IMessage>();
            
            _messagePublisher.Dispose();

            await _messagePublisher.Awaiting(x => x.PublishAsync(messageMock.Object, CancellationToken.None))
                .Should()
                .ThrowAsync<ObjectDisposedException>();
        }
        
        [Fact]
        public void GivenSubscriberWhenUnsubscribeShouldRemoveObserver()
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
        public async Task GivenMessageWhenCancellationRequestedShouldSkipMessage()
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
            
            cancellationTokenSource.Cancel();
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            _channelMock.Verify(
                x => x.WaitForConfirmsOrDie(It.IsAny<TimeSpan>()),
                Times.Never());
            _channelMock.VerifyAll();
            
            messageMock.VerifyAll();
            batchMock.Verify(x => x.Publish(), Times.Never());
            batchMock.VerifyAll();
        }
    }
}