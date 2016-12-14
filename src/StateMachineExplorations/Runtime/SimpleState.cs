namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SimpleState : EventStateBase
    {
        private TaskCompletionSource<Transition> taskCompletionSource;
        private readonly List<Transition> transitions = new List<Transition>();
        private Task<Transition> executingTask;

        public SimpleState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExecuteAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.OnExecuteAction = onExecuteAction;
        }

        public Func<string, Task> OnExecuteAction { get; }

        protected override async Task<Transition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnExecuteAction != null)
            {
                await this.OnExecuteAction(this.Name);
            }

            return null;
        }
    }
}
