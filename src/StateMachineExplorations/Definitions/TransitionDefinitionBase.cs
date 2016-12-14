namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Threading.Tasks;

    public abstract class TransitionDefinitionBase
    {
        protected TransitionDefinitionBase(string name, string triggerName)
        {
            this.Name = name;
            this.TriggerName = triggerName;
        }

        public string Name { get; }

        public string TriggerName { get; }

        public Func<string, Task> Action { get; set; }
    }
}
