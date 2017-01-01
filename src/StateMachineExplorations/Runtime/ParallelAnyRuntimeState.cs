namespace Morgados.StateMachine.Runtime
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
            Func<string, Task> onCancelledAction,
            IEnumerable<RuntimeStateBase> regions)
            : base(name, onEnterAction, onExitAction, onCancelledAction, regions)
        {
        }

        protected override async Task<RuntimeTransition> ExecuteRegionsAsync(CancellationToken cancellationToken, IEnumerable<RuntimeStateBase> regions)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    cancellationTokenSource.Token))
                {
                    var tasks = regions
                        .Select(async s => 
                            {
                                await s.ExecuteAsync(combinedCancellationTokenSource.Token);
                                return s;
                            })
                        .ToArray();

                    var task = await Task.WhenAny(tasks);

                    await (await task).ExitOrCancelAsync(cancellationToken);

                    combinedCancellationTokenSource.Cancel(true);

                    await Task.WhenAll(
                        (await Task.WhenAll(tasks.Where(t => t != task)))
                        .Select(s => s.ExitOrCancelAsync(combinedCancellationTokenSource.Token)));
                }
            }

            return null;
        }
    }
}
