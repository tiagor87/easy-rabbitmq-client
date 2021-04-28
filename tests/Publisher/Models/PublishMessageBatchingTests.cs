using System;
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
    public class PublishMessageBatchingTests
    {
        [Fact]
        public void GivenBatching_ShouldInstantiate()
        {
            var publisherMock = new Mock<IPublisher>();
            var messageMock = new Mock<IPublisherMessage>();
            var timespan = TimeSpan.FromSeconds(5);

            var batching = new PublisherMessageBatching(publisherMock.Object, new[] {messageMock.Object}, timespan);

            batching.Should().NotBeNull();
            batching.Publisher.Should().Be(publisherMock.Object);
            batching.PublishingTimeout.Should().Be(timespan);
        }

        [Fact]
        public async Task GivenBatching_WhenPublish_ShouldPublish()
        {
            var publisherMock = new Mock<IPublisher>();
            var messageMock = new Mock<IPublisherMessage>();
            var cancellationToken = new CancellationToken();
            publisherMock.Setup(x => x.PublishAsync(It.IsAny<IPublisherMessageBatching>(), cancellationToken))
                .Verifiable();

            var batching = new PublisherMessageBatching(publisherMock.Object, messageMock.Object);

            await batching.PublishAsync(cancellationToken);

            publisherMock.VerifyAll();
        }
    }
}