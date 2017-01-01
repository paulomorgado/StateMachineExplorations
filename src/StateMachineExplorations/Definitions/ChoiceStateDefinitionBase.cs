namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class ChoiceStateDefinitionBase : StateDefinitionBase
    {
        protected ChoiceStateDefinitionBase(string name)
            : base(name)
        {
        }

        public TransitionDefinition ElseTransition { get; set; }
    }
}
