namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ParallelAnyState : ParallelStateBase
    {
        public ParallelAnyState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            IEnumerable<SimpleState> subStates)
            : base(name, onEnterAction, onExitAction, onCancelledAction, subStates)
        {
        }

        protected override async Task WhenSubStates(IEnumerable<Task<Transition>> subStates)
        {
            await await Task.WhenAny(subStates);
        }
    }
}
