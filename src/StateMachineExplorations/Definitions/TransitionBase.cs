namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class TransitionBase
    {
        protected TransitionBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public Func<CancellationToken, string, string, Task> Action { get; set; }
    }
}
