namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParallelAllState : ParallelStateBase
    {
        public ParallelAllState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<StateBase> regions)
            : base(name, onEnterAction, onExitAction, onCancelledAction, regions)
        {
        }

        protected override async Task<Transition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<StateBase> regions)
        {
            await Task.WhenAll(regions.Select(s => s.ExecuteAsync(cancellationToken))).ConfigureAwait(false);

            return null;
        }
    }
}
