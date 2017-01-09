namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ParallelAnyRuntimeState : ParallelRuntimeStateBase
    {
        public ParallelAnyRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction,
            IEnumerable<RuntimeStateBase> regions)
            : base(name, onEnterAction, onExitAction, onCanceledAction, regions)
        {
        }

        protected override async Task<RuntimeTransition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<RuntimeStateBase> regions)
        {
            using (var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    CancellationToken.None))
            {
                var tasks = regions
                    .Select(r => r.OnExecuteAsync(cancellationTokenSource.Token))
                    .ToArray();

                var first = await Task.WhenAny(tasks);

                var transition = await (await first)();

                cancellationTokenSource.Cancel(true);

                await Task.WhenAll(
                    tasks
                        .Select(async t => await (await t)()));
            }

            return null;
        }
    }
}
