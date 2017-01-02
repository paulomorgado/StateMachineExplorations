namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParallelAllRuntimeState : ParallelRuntimeStateBase
    {
        public ParallelAllRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<RuntimeStateBase> regions)
            : base(name, onEnterAction, onExitAction, onCancelledAction, regions)
        {
        }

        protected override async Task<RuntimeTransition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<RuntimeStateBase> regions)
        {
            await Task.WhenAll(regions.Select(s => s.ExecuteAsync(cancellationToken))).ConfigureAwait(false);

            return null;
        }
    }
}
