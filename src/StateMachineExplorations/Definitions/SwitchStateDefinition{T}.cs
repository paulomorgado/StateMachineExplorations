namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class SwitchStateDefinition<T> : ChoiceStateDefinitionBase
    {
        protected SwitchStateDefinition(string name)
            : base(name)
        {
        }

        public IDictionary<T, TransitionDefinition> SelectionTransitions { get; } = new Dictionary<T, TransitionDefinition>();
    }
}
