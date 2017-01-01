namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class StateDefinitionBase
    {
        protected StateDefinitionBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
