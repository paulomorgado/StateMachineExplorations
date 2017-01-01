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
            var logger = new TestLogger();

            var tcs2 = new TaskCompletionSource<object>();

            var state1 = new SimpleState(
                "state1",
                logger.StateEnterAction,
                logger.StateExecutionAction,
                logger.StateExitAction,
                logger.StateCancelledAction);

            var state2 = new SimpleState(
                "state2",
                logger.StateEnterAction,
                async s => { await logger.StateExecutionAction(s); tcs2.SetResult(null); },
                logger.StateExitAction,
                logger.StateCancelledAction);

            var state3 = new SimpleState(
                "state3",
                logger.StateEnterAction,
                logger.StateExecutionAction,
                logger.StateExitAction,
                logger.StateCancelledAction);

            var compositeState = new CompositeState(
                "composite",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                new Transition("T1", state1, logger.TransitionAction, null));

            state1.AddEventTransition("E1", new Transition("T2", state2, logger.TransitionAction, null));
            state2.AddEventTransition("E2", new Transition("T3", state3, logger.TransitionAction, null));

            var task = compositeState.ExecuteAsync(CancellationToken.None);

            Assert.False(task.IsCompleted);

            var handled = await compositeState.PublishEventAsync("E1");

            Assert.True(handled);
            Assert.False(task.IsCompleted);

            await tcs2.Task;

            handled = await compositeState.PublishEventAsync("E2");

            Assert.True(handled);

            await task;

            Assert.Equal(">composite;@composite->state1;>state1;*state1;<state1;@state1->state2;>state2;*state2;<state2;@state2->state3;>state3;*state3;<state3;<composite;", logger.ToString());
        }
    }
}
