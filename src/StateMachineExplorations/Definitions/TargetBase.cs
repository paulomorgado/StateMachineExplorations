namespace Morgados.StateMachines.Definitions
{

    public abstract class TargetBase
    {
        protected TargetBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
