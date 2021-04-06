using System.Text;
using EasyRabbitMqClient.Abstractions.Publishers;
using Newtonsoft.Json;

namespace EasyRabbitMqClient.Core.Tests.Serializers
{
    public class JsonSerializer : IPublisherSerializer
    {
        public byte[] Serialize(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }
        
        public void Dispose()
        {
            // no implementation
        }
    }
}