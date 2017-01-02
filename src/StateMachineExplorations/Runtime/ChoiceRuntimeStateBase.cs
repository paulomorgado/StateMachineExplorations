namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ChoiceRuntimeStateBase : RuntimeStateBase
    {
        private readonly RuntimeTransition elseTransition;

        protected ChoiceRuntimeStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            RuntimeTransition elseTransition)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.elseTransition = elseTransition;
        }

        protected override async Task<RuntimeTransition> ExecuteStepAsync(CancellationToken cancellationToken)
        {
            var transition = this.SelectTransition();

            return cancellationToken.IsCancellationRequested
                ? null
                : transition ?? this.elseTransition;
        }

        protected abstract RuntimeTransition SelectTransition();
    }
}
