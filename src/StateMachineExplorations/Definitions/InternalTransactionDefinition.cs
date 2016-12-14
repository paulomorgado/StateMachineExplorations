namespace Morgados.StateMachine.Definitions
{
    public class InternalTransactionDefinition : TriggeredTransactionDefinition
    {
        public InternalTransactionDefinition(string name, string triggerName, string targetName)
            : base(name, triggerName, targetName)
        {
        }
    }
}
