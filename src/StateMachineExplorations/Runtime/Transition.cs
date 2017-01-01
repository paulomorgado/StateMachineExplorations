namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay("TargetType = { Target?.GetType().Name }, TargetName = {Target?.Name}")]
    public class Transition
    {
        private readonly Func<CancellationToken, string, string, Task> action;
        private readonly Func<string, bool> guard;

        public Transition(
            string name,
            ITransitionTarget target,
            Func<CancellationToken, string, string, Task> action,
            Func<string, bool> guard)
        {
            this.Name = name;
            this.Target = target;
            this.guard = guard;
            this.action = action;
        }

        public string Name { get; }

        public ITransitionTarget Target { get; }

        public bool Guard(string soruce) => this.guard?.Invoke(soruce) ?? true;

        public Task ExecuteActionAsync(CancellationToken cancellationToken, string soruce, string target)
            => this.action?.Invoke(cancellationToken, soruce, target);
    }
}