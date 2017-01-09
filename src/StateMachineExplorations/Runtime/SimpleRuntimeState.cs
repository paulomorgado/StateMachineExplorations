namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SimpleRuntimeState : EventRuntimeStateBase
    {
        private TaskCompletionSource<RuntimeTransition> taskCompletionSource;
        private readonly List<RuntimeTransition> transitions = new List<RuntimeTransition>();
        private Task<RuntimeTransition> executingTask;

        public SimpleRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExecuteAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction)
            : base(name, onEnterAction, onExitAction, onCanceledAction)
        {
            this.OnExecuteAction = onExecuteAction;
        }

        public Func<string, Task> OnExecuteAction { get; }

        protected override async Task<RuntimeTransition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnExecuteAction != null)
            {
                await this.OnExecuteAction(this.Name);
            }

            return null;
        }
    }
}
