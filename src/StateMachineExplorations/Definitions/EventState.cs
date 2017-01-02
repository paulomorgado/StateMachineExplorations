namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Runtime;

    public abstract class EventState : StateBase
    {
        protected EventState(string name)
            : base(name)
        {
        }

        public IDictionary<string, ICollection<GuardedTransitionBase>> Transitions { get; } = new Dictionary<string, ICollection<GuardedTransitionBase>>();
    }
}
