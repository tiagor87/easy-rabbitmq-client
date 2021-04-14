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
using EasyRabbitMqClient.Publisher.Exceptions;
using EasyRabbitMqClient.Publisher.Tests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Publisher.Tests
{
    public class MessagePublisherTests : IClassFixture<ConnectionFixture>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly Mock<IPublisherBehavior> _behaviorMock;

        public MessagePublisherTests(ConnectionFixture connectionFixture)
        {
            _behaviorMock = new Mock<IPublisherBehavior>();
            _messagePublisher = new MessagePublisher(_behaviorMock.Object);
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
        public async Task GivenMessage_ShouldPublishAndConfirm()
        {
            var messageMock = new Mock<IMessage>();
            SetupProxyPassBehavior();
            
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
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
            var messageMock = new Mock<IMessage>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            _behaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IMessageBatching>(),
                        It.IsAny<Func<IMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Throws(new PublishingException(new MessageBatching(new []{ messageMock.Object }), new Exception()))
                .Verifiable();
            
            messageMock.SetupSequence(x => x.Equals(It.IsAny<IMessage>()))
                .Returns(false)
                .Returns(true)
                .Returns(false);

            using var _ = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishBatchingAsync(new MessageBatching(new [] { messageMock.Object, messageMock.Object, messageMock.Object }), CancellationToken.None);
            
            messageMock.VerifyAll();
            _behaviorMock.VerifyAll();
            observerMock.Verify(x => x.OnNext(It.Is<IMessageBatching>(y => y.Count == 2)));
            observerMock.Verify(x => x.OnError(It.Is<PublishingException>(y => y.Batching.Count == 1)));
        }
        
        [Fact]
        public async Task GivenMessage_WhenPublish_ShouldNotifyObservers()
        {
            var messageMock = new Mock<IMessage>();
            var observerMock = new Mock<IObserver<IMessageBatching>>();
            
            _behaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IMessageBatching>(),
                        It.IsAny<Func<IMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            using var _ = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishBatchingAsync(new MessageBatching(new [] { messageMock.Object, messageMock.Object, messageMock.Object }), CancellationToken.None);
            
            messageMock.VerifyAll();
            _behaviorMock.VerifyAll();
            observerMock.Verify(x => x.OnNext(It.Is<IMessageBatching>(y => y.Count == 3)));
        }
        
        [Theory]
        [MemberData(nameof(GetExceptions))]
        public async Task GivenMessage_WhenFails_ShouldCreateExceptionWithMessageBatching(Exception exception)
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
            
            using var _ = _messagePublisher.Subscribe(observerMock.Object);
            await _messagePublisher.PublishAsync(messageMock.Object, CancellationToken.None);
            
            messageMock.VerifyAll();
            _behaviorMock.VerifyAll();
            observerMock.Verify(x => x.OnError(It.Is<PublishingException>(y => y.Batching.Count == 1)));
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