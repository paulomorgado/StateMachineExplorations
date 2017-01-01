namespace Morgados.StateMachine.Definitions
{

    public abstract class TargetDefinitionBase
    {
        protected TargetDefinitionBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
