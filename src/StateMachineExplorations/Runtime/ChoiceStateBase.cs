namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ChoiceStateBase : StateBase
    {
        private readonly Transition elseTransition;

        protected ChoiceStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            Transition elseTransition)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.elseTransition = elseTransition;
        }

        protected override Task<Transition> ExecuteStepAsync(CancellationToken cancellationToken)
            => Task.FromResult(this.SelectTransition() ?? this.elseTransition);

        protected abstract Transition SelectTransition();
    }
}
