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
            IEnumerable<EventStateBase> subStates)
            : base(name, onEnterAction, onExitAction, onCancelledAction, subStates)
        {
        }

        protected override async Task WhenSubStates(IEnumerable<Task<Transition>> subStates)
        {
            await Task.WhenAll(subStates);
        }
    }
}
