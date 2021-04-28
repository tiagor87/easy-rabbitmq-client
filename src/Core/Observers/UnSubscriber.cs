using System;

namespace EasyRabbitMqClient.Core.Observers
{
    public class UnSubscriber : IDisposable
    {
        private readonly Action _unsubscribe;

        public UnSubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe();
        }
    }
}