namespace Morgados.StateMachine.Definitions
{
    using System.Collections.Generic;

    public class CompositeStateDefinition : StateDefinition
    {
        public CompositeStateDefinition(string name)
            : base(name)
        {
        }

        public string InitialState { get; set; }

        public ICollection<StateDefinition> SubStates { get; } = new List<StateDefinition>();
    }
}
