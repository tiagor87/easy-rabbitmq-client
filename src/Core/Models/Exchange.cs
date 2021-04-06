using EasyRabbitMqClient.Abstractions.Models;

namespace EasyRabbitMqClient.Core.Models
{
    public class Exchange : IExchange
    {
        public Exchange(string name, string type, bool isDurable, bool isAutoDelete, bool isInternal,
            IExchange fallbackExchange)
        {
            Name = name;
            Type = type;
            IsDurable = isDurable;
            IsAutoDelete = isAutoDelete;
            IsInternal = isInternal;
            FallbackExchange = fallbackExchange;
        }
        
        public string Name { get; }
        public string Type { get; }
        public bool IsDurable { get; }
        public bool IsAutoDelete { get; }
        public bool IsInternal { get; }
        public IExchange FallbackExchange { get; }
    }
}