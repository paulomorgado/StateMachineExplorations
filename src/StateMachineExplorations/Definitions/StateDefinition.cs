namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StateDefinition
    {
        public StateDefinition(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public ICollection<TriggeredTransactionDefinition> Transitions { get; } = new List<TriggeredTransactionDefinition>();

        public Func<string, Task> OnEnterAction { get; set; }

        public Func<string, Task> OnExitAction { get; set; }

        public Func<string, Task> OnCancelledAction { get; set; }
    }
}
