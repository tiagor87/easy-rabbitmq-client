using System;
using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions.Behaviors;
using EasyRabbitMqClient.Core.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasyRabbitMqClient.Core.Tests.Extensions
{
    public class BehaviorExtensionsTests
    {
        [Fact]
        public void GivenBehaviors_WhenThrow_ShouldPassException()
        {
            var behaviorMock = new Mock<IBehavior>();
            
            var failureBehaviorMock = new Mock<IBehavior>();
            failureBehaviorMock.Setup(x => x.Execute(It.IsAny<Func<bool>>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            ICollection<IBehavior> behaviors = new[] {behaviorMock.Object, failureBehaviorMock.Object};
            
            behaviors.Invoking(x => x.Execute(null)).Should().Throw<InvalidOperationException>();
            
            behaviorMock.Verify(x => x.Execute(It.IsAny<Func<bool>>()), Times.Never());
            failureBehaviorMock.VerifyAll();
        }
        
        [Fact]
        public void GivenBehaviors_ShouldExecuteEveryone()
        {
            var behaviorMock = new Mock<IBehavior>();
            behaviorMock.Setup(x => x.Execute(It.IsAny<Func<bool>>()))
                .Returns((Func<bool> func) => func())
                .Verifiable();

            var executed = false;
            Func<bool> action = () => executed = true;

            ICollection<IBehavior> behaviors = new[] {behaviorMock.Object, behaviorMock.Object, behaviorMock.Object};

            var result = behaviors.Execute(action);

            result.Should().BeTrue();
            executed.Should().BeTrue();
            behaviorMock.VerifyAll();
        }
    }
}