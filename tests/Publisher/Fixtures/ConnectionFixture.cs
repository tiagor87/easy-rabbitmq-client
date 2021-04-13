using Moq;
using RabbitMQ.Client;

namespace EasyRabbitMqClient.Publisher.Tests.Fixtures
{
    public class ConnectionFixture
    {
        public IConnectionFactory GetConnectionFactory(out Mock<IConnection> connectionMock, out Mock<IModel> channelMock)
        {
            channelMock = new Mock<IModel>();
            connectionMock = new Mock<IConnection>();
            connectionMock.Setup(x => x.CreateModel())
                .Returns(channelMock.Object);
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            connectionFactoryMock.Setup(x => x.CreateConnection())
                .Returns(connectionMock.Object);
            return connectionFactoryMock.Object;
        }
    }
}