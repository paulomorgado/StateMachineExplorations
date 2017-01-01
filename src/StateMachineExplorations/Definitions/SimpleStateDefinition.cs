namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SimpleStateDefinition : StateDefinitionBase
    {
        public SimpleStateDefinition(string name)
            : base(name)
        {
        }

        public IDictionary<string, ICollection<GuardedTransitionDefinitionBase>> Transitions { get; } = new Dictionary<string, ICollection<GuardedTransitionDefinitionBase>>();

        public Func<string, Task> OnEnterAction { get; set; }

        public Func<string, Task> OnExitAction { get; set; }

        public Func<string, Task> OnCancelledAction { get; set; }
    }
}
