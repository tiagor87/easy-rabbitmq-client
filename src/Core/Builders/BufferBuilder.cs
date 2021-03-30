using System;
using System.Collections.Generic;
using EasyRabbitMqClient.Abstractions.Builders;
using TRBufferList.Core;

namespace EasyRabbitMqClient.Core.Builders
{
    public class BufferBuilder<T> : IBufferBuilder<T>
    {
        private int _capacity;
        private TimeSpan _drainTimeout;
        private int? _maxSize;
        private int _maxFaultSize;
        private TimeSpan _idleTtl;

        public IBufferBuilder<T> WithCapacity(int capacity)
        {
            _capacity = capacity;
            return this;
        }

        public IBufferBuilder<T> WithMaxSize(int? maxSize)
        {
            _maxSize = maxSize;
            return this;
        }

        public IBufferBuilder<T> WithMaxFaultSize(int maxFaultSize)
        {
            _maxFaultSize = maxFaultSize;
            return this;
        }

        public IBufferBuilder<T> WithIdleTtl(TimeSpan idleTtl)
        {
            _idleTtl = idleTtl;
            return this;
        }

        public IBufferBuilder<T> DrainTimeout(TimeSpan drainTimeout)
        {
            _drainTimeout = drainTimeout;
            return this;
        }

        public ICollection<T> Build()
        {
            var options = new BufferListOptions(_capacity, _maxSize, _maxFaultSize, _idleTtl, TimeSpan.FromSeconds(1),
                _drainTimeout);
            return new BufferList<T>(options);
        }
    }
}