namespace Morgados.StateMachine.Definitions
{
    using System.Collections.Generic;

    public class CompositeStateDefinition : SimpleStateDefinition
    {
        public CompositeStateDefinition(string name)
            : base(name)
        {
        }

        public string InitialState { get; set; }

        public ICollection<SimpleStateDefinition> SubStates { get; } = new List<SimpleStateDefinition>();
    }
}
