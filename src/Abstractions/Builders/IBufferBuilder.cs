using System;
using System.Collections.Generic;

namespace EasyRabbitMqClient.Abstractions.Builders
{
    public interface IBufferBuilder<T>
    {
        IBufferBuilder<T> WithCapacity(int capacity);
        IBufferBuilder<T> WithMaxSize(int? maxSize);
        IBufferBuilder<T> WithMaxFaultSize(int maxFaultSize);
        IBufferBuilder<T> WithIdleTtl(TimeSpan idleTtl);
        IBufferBuilder<T> DrainTimeout(TimeSpan drainTimeout);
        ICollection<T> Build();
    }
}