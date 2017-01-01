namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class IfElseStateDefinition : ChoiceStateDefinitionBase
    {
        protected IfElseStateDefinition(string name)
            : base(name)
        {
        }

        public TransitionDefinition TrueTransition { get; set; }
    }
}
