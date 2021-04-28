using System;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Abstractions.Publishers.Models;
using EasyRabbitMqClient.Core.Behaviors;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Core.Tests.Behaviors
{
    public class AggregateBehaviorTests
    {
        [Fact]
        public void GivenBehaviors_WhenThrow_ShouldPassException()
        {
            var behaviorMock = new Mock<IBehavior<IPublisherMessageBatching>>();

            var failureBehaviorMock = new Mock<IBehavior<IPublisherMessageBatching>>();
            failureBehaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IPublisherMessageBatching>(),
                        It.IsAny<Func<IPublisherMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            var behavior =
                AggregateBehavior<IPublisherMessageBatching>.Create(behaviorMock.Object, failureBehaviorMock.Object);

            behavior.Awaiting(x => x.ExecuteAsync(null, null, CancellationToken.None))
                .Should()
                .Throw<InvalidOperationException>();

            behaviorMock.Verify(x =>
                x.ExecuteAsync(
                    It.IsAny<IPublisherMessageBatching>(),
                    It.IsAny<Func<IPublisherMessageBatching, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()), Times.Never());
            failureBehaviorMock.VerifyAll();
        }

        [Fact]
        public async Task GivenBehaviors_ShouldExecuteEveryone()
        {
            var behaviorMock = new Mock<IBehavior<IPublisherMessageBatching>>();
            behaviorMock.Setup(x =>
                    x.ExecuteAsync(
                        It.IsAny<IPublisherMessageBatching>(),
                        It.IsAny<Func<IPublisherMessageBatching, CancellationToken, Task>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            using var stub1 = new StubBehavior();
            using var stub2 = new StubBehavior();
            using var stub3 = new StubBehavior();
            using var behavior =
                AggregateBehavior<IPublisherMessageBatching>.Create(behaviorMock.Object, stub1, stub2, stub3);
            await behavior.ExecuteAsync(null, null, CancellationToken.None);

            StubBehavior.Executed.Should().Be(3);
            behaviorMock.VerifyAll();
        }
    }

    internal class StubBehavior : IBehavior<IPublisherMessageBatching>
    {
        public static int Executed { get; private set; }

        public void Dispose()
        {
        }

        public async Task ExecuteAsync(IPublisherMessageBatching batching,
            Func<IPublisherMessageBatching, CancellationToken, Task> next, CancellationToken cancellationToken)
        {
            Executed++;
            await next(batching, cancellationToken);
        }
    }
}