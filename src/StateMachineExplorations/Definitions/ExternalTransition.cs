namespace Morgados.StateMachines.Definitions
{
    public class ExternalTransition : GuardedTransitionBase
    {
        public ExternalTransition(string name)
            : base(name)
        {
        }

        public TargetBase Target { get; set; }
    }
}
