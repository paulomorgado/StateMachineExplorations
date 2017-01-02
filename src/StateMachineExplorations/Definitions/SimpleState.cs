namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SimpleState : StateBase
    {
        public SimpleState(string name)
            : base(name)
        {
        }

        public Func<string, Task> OnExecuteAction { get; set; }
    }
}
