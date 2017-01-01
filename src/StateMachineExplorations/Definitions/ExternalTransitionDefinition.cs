namespace Morgados.StateMachine.Definitions
{
    public class ExternalTransitionDefinition : GuardedTransitionDefinitionBase
    {
        public ExternalTransitionDefinition(string name)
            : base(name)
        {
        }

        public TargetDefinitionBase Target { get; set; }
    }
}
