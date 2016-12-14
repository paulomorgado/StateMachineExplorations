namespace Morgados.StateMachine.Definitions
{
    public class ExternalTransactionDefinition : TriggeredTransactionDefinition
    {
        public ExternalTransactionDefinition(string name, string triggerName, string targetName)
            : base(name, triggerName, targetName)
        {
        }
    }
}
