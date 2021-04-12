using System;

namespace EasyRabbitMqClient.Publisher.Observers
{
    internal class UnSubscriber : IDisposable
    {
        private readonly Action _unsubscribe;

        internal UnSubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe();
        }
    }
}