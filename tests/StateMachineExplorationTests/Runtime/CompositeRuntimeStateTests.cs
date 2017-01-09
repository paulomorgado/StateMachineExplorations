namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using Morgados.StateMachines.Runtime;
    using Xunit;
    using System.Threading.Tasks;

    public class CompositeRuntimeStateTests
    {
        [Fact]
        public async Task CompositeRuntimeState_WithoutCancellationAndEventTransitions_ExecutesAllSubStates()
        {
            var tracker = new TestTracker();

            var tcs2 = new TaskCompletionSource<object>();

            var state1 = new SimpleRuntimeState(
                "state1",
                tracker.StateEnterAction,
                tracker.StateExecutionAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var state2 = new SimpleRuntimeState(
                "state2",
                tracker.StateEnterAction,
                async s =>
                {
                    await tracker.StateExecutionAction(s);
                    tcs2.SetResult(null);
                },
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var state3 = new SimpleRuntimeState(
                "state3",
                tracker.StateEnterAction,
                tracker.StateExecutionAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var compositeState = new CompositeRuntimeState(
                "composite",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                new RuntimeTransition("T1", state1, tracker.TransitionAction, null));

            state1.AddEventTransition("E1", new RuntimeTransition("T2", state2, tracker.TransitionAction, null));
            state2.AddEventTransition("E2", new RuntimeTransition("T3", state3, tracker.TransitionAction, null));

            var task = compositeState.ExecuteAsync(CancellationToken.None);

            Assert.False(task.IsCompleted);

            var handled = await compositeState.PublishEventAsync("E1");

            Assert.True(handled);
            Assert.False(task.IsCompleted);

            await tcs2.Task;

            handled = await compositeState.PublishEventAsync("E2");

            Assert.True(handled);

            await task;

            Assert.Equal(">composite;@composite->state1;>state1;*state1;!state1;@state1->state2;>state2;*state2;!state2;@state2->state3;>state3;*state3;<state3;<composite;", tracker.ToString());
        }
    }
}
