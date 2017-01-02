namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class ParallelRuntimeStateBase : EventRuntimeStateBase
    {
        private readonly IEnumerable<RuntimeStateBase> regions;

        protected ParallelRuntimeStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<RuntimeStateBase> regions)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
            this.regions = regions;
        }

        protected override async Task<RuntimeTransition> ExecuteEventStepAsync(CancellationToken cancellationToken)
        {
            return await ExecuteRegionsAsync(cancellationToken, this.regions);
        }

        protected abstract Task<RuntimeTransition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<RuntimeStateBase> regions);

        protected internal override async Task<bool?> OnPublishEventAsync(string eventName)
            => (await Task.WhenAll(this.regions.OfType<EventRuntimeStateBase>().Select(s => s.OnPublishEventAsync(eventName)))).Any(a => a.GetValueOrDefault())
                ? true
                : await base.OnPublishEventAsync(eventName);
    }
}
