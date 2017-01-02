namespace Morgados.StateMachines.Definitions
{
    using System.Collections.Generic;

    public class ParallelState : EventState
    {
        public ParallelState(string name)
            : base(name)
        {
        }

        public ParallelModes Mode { get; set; }

        public ICollection<StateBase> Regions { get; } = new List<StateBase>();
    }
}
