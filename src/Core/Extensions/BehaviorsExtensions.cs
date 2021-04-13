using System;
using System.Collections.Generic;
using System.Linq;
using EasyRabbitMqClient.Abstractions.Behaviors;

namespace EasyRabbitMqClient.Core.Extensions
{
    public static class BehaviorsExtensions
    {
        public static bool Execute(this ICollection<IBehavior> behaviors,  Func<bool> action)
        {
            var i = 0;
            IBehavior behavior = null;
            do
            {
                behavior = new AggregateBehavior(behaviors.ElementAt(i), behavior);
                i++;
            } while (i < behaviors.Count);

            return behavior.Execute(action);
        }
        
        class AggregateBehavior : IBehavior
        {
            private readonly IBehavior _behavior;
            private readonly IBehavior _nextBehavior;

            public AggregateBehavior(IBehavior behavior, IBehavior nextBehavior)
            {
                _behavior = behavior;
                _nextBehavior = nextBehavior;
            }
            
            public bool Execute(Func<bool> next)
            {
                return _behavior.Execute(() => _nextBehavior?.Execute(next) ?? next());
            }
        }
    }
}