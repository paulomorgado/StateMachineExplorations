namespace Morgados.StateMachines.Definitions
{
    public class Transition : TransitionBase
    {
        public Transition(string name)
            : base(name)
        {
        }

        public TargetBase Target { get; set; }
    }
}
