namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using Morgados.StateMachine.Runtime;
    using Xunit;
    using System.Threading.Tasks;

    public class CompositeStateTests
    {
        [Fact]
        public async Task CompositeState_WithoutCancellationAndEventTransitions_ExecutesAllSubStates()
        {
            var tracker = new TestTracker();

            var tcs2 = new TaskCompletionSource<object>();

            var state1 = new SimpleState(
                "state1",
                tracker.StateEnterAction,
                tracker.StateExecutionAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction);

            var state2 = new SimpleState(
                "state2",
                tracker.StateEnterAction,
                async s => { await tracker.StateExecutionAction(s); tcs2.SetResult(null); },
                tracker.StateExitAction,
                tracker.StateCancelledAction);

            var state3 = new SimpleState(
                "state3",
                tracker.StateEnterAction,
                tracker.StateExecutionAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction);

            var compositeState = new CompositeState(
                "composite",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction,
                new Transition("T1", state1, tracker.TransitionAction, null));

            state1.AddEventTransition("E1", new Transition("T2", state2, tracker.TransitionAction, null));
            state2.AddEventTransition("E2", new Transition("T3", state3, tracker.TransitionAction, null));

            var task = compositeState.ExecuteAsync(CancellationToken.None);

            Assert.False(task.IsCompleted);

            var handled = await compositeState.PublishEventAsync("E1");

            Assert.True(handled);
            Assert.False(task.IsCompleted);

            await tcs2.Task;

            handled = await compositeState.PublishEventAsync("E2");

            Assert.True(handled);

            await task;

            Assert.Equal(">composite;@composite->state1;>state1;*state1;<state1;@state1->state2;>state2;*state2;<state2;@state2->state3;>state3;*state3;<state3;<composite;", tracker.ToString());
        }
    }
}
