namespace Morgados.StateMachines.Definitions
{
    using System;

    public abstract class GuardedTransitionBase : TransitionBase
    {
        protected GuardedTransitionBase(string name)
            : base(name)
        {
        }

        public Func<string, bool> Guard { get; set; }
    }
}
