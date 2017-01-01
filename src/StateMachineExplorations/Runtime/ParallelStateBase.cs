namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ParallelStateBase : EventStateBase
    {
        private readonly IEnumerable<StateBase> regions;

        protected ParallelStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<StateBase> regions)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.regions = regions;
        }

        protected override async Task<Transition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            return await ExecuteRegionsAsync(cancellationToken, this.regions);
        }

        protected abstract Task<Transition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<StateBase> regions);

        protected internal override async Task<bool?> OnPublishEventAsync(string eventName)
            => (await Task.WhenAll(this.regions.OfType<EventStateBase>().Select(s => s.OnPublishEventAsync(eventName)))).Any(a => a.GetValueOrDefault())
                ? true
                : await base.OnPublishEventAsync(eventName);
    }
}
