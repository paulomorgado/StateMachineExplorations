namespace Morgados.StateMachine.Definitions
{
    public class TransitionDefinition : TransitionDefinitionBase
    {
        public TransitionDefinition(string name)
            : base(name)
        {
        }

        public TargetDefinitionBase Target { get; set; }
    }
}
