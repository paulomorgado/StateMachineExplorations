namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ParallelStateBase : EventStateBase
    {
        private readonly IEnumerable<StateBase> subStates;

        protected ParallelStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<StateBase> subStates)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.subStates = subStates;
        }

        protected override async Task<Transition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    cancellationTokenSource.Token))
                {
                    await this.WhenSubStates(this.subStates.Select(s => s.ExecuteAsync(cancellationToken)));

                    cancellationTokenSource.Cancel();
                }
            }

            return null;
        }

        protected abstract Task WhenSubStates(IEnumerable<Task<Transition>> subStates);

        protected internal override async Task<bool> OnPublishEventAsync(string eventName) 
            => (await Task.WhenAll(
                this.subStates
                    .OfType<EventStateBase>()
                    .Select(s => s.OnPublishEventAsync(eventName))
                )).Any(a => a)
                && await base.OnPublishEventAsync(eventName);
    }
}
