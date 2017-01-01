using System.Collections.Generic;

namespace Morgados.StateMachine.Definitions
{
    public class ParallelStateDefinition : SimpleStateDefinition
    {
        public ParallelStateDefinition(string name)
            : base(name)
        {
        }

        public ParallelModes Mode { get; set; }

        public ICollection<SimpleStateDefinition> SubStates { get; } = new List<SimpleStateDefinition>();
    }
}
