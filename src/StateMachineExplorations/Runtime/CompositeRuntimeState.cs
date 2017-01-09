namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay("Name = {Name}, Current = {currentSubState?.Name}, Executing = {isExecuting != 0}")]
    public class CompositeRuntimeState : EventRuntimeStateBase
    {
        private readonly RuntimeTransition initialTransition;
        private RuntimeStateBase currentSubState;
        private int isExecuting;

        public CompositeRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction,
            RuntimeTransition initialTransition)
            : base(name, onEnterAction, onExitAction, onCanceledAction)
        {
            this.initialTransition = initialTransition;
        }

        protected override async Task<RuntimeTransition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            try
            {
                Interlocked.Exchange(ref this.isExecuting, 1);

                this.currentSubState = null;
                var transition = this.initialTransition;

                while (transition != null)
                {
                    var nextState = transition.Target as RuntimeStateBase;

                    await transition.ExecuteActionAsync(cancellationToken, (this.currentSubState ?? this)?.Name, nextState.Name);

                    Interlocked.Exchange(ref this.currentSubState, nextState);

                    if (this.currentSubState == null)
                    {
                        return null;
                    }

                    transition = await this.currentSubState.ExecuteAsync(cancellationToken);
                }

                return null;
            }
            finally
            {
                Interlocked.Exchange(ref this.isExecuting, 0);
            }
        }

        /// <inheritdoc/>
        protected internal override async Task<bool?> OnPublishEventAsync(string eventName)
        {
            while (this.isExecuting != 0)
            {
                if (this.currentSubState is EventRuntimeStateBase eventState)
                {
                    var status = await eventState.OnPublishEventAsync(eventName);

                    if (!status.HasValue)
                    {
                        continue;
                    }
                    else
                    {
                        if (status.Value)
                        {
                            return true;
                        }
                    }
                }

                return await base.OnPublishEventAsync(eventName);
            }

            return null;
        }
    }
}
