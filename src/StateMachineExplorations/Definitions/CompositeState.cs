namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;

    public class CompositeState : EventState
    {
        public CompositeState(string name)
            : base(name)
        {
        }

        public Transition InitialTransition { get; set; }

        public ICollection<SimpleState> SubStates { get; } = new List<SimpleState>();
    }
}
