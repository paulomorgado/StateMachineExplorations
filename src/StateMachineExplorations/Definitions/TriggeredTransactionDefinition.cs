namespace Morgados.StateMachine.Definitions
{
    using System;

    public abstract class TriggeredTransactionDefinition : TransitionDefinitionBase
    {
        protected TriggeredTransactionDefinition(string name, string triggerName, string targetName)
            : base(name, triggerName)
        {
            this.TargetName = targetName;
        }

        public Func<string, string, bool> Guard { get; set; }

        public string TargetName { get; }
    }
}
