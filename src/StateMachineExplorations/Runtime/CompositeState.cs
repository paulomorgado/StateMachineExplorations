namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CompositeState : EventStateBase
    {
        private StateBase currentSubState;
        private readonly Transition initialTransition;

        public CompositeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            Transition initialSubState)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.initialTransition = initialSubState;
        }

        protected override async Task<Transition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            this.currentSubState = null;
            var transition = this.initialTransition;

            while (transition != null)
            {
                var nextState = transition.Target as StateBase;

                await transition.ExecuteActionAsync(cancellationToken, (this.currentSubState ?? this)?.Name, nextState.Name);

                this.currentSubState = nextState;

                if (this.currentSubState == null)
                {
                    return null;
                }

                transition = await this.currentSubState.ExecuteAsync(cancellationToken);
            }

            return null;
        }

        protected internal override async Task<bool> OnPublishEventAsync(string eventName)
            => (this.currentSubState is EventStateBase eventState && await eventState.OnPublishEventAsync(eventName))
                || await base.OnPublishEventAsync(eventName);
    }
}
