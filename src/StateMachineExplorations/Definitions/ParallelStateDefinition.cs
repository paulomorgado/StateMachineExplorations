using System.Collections.Generic;

namespace Morgados.StateMachine.Definitions
{
    public class ParallelStateDefinition : StateDefinition
    {
        public ParallelStateDefinition(string name)
            : base(name)
        {
        }

        public ParallelModes Mode { get; set; }

        public ICollection<StateDefinition> SubStates { get; } = new List<StateDefinition>();
    }
}
