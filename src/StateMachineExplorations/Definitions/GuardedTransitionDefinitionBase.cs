namespace Morgados.StateMachine.Definitions
{
    using System;

    public abstract class GuardedTransitionDefinitionBase : TransitionDefinitionBase
    {
        protected GuardedTransitionDefinitionBase(string name)
            : base(name)
        {
        }

        public Func<string, string, bool> Guard { get; set; }
    }
}
