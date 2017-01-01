namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using Morgados.StateMachine.Runtime;
    using Xunit;
    using System.Threading.Tasks;

    public class ParallelAllStateTests
    {
        [Fact]
        public async Task ParallelAllState_WithoutCancellationAndEventTransitions_ExecutesAllSubStates()
        {
            var logger = new TestLogger();

            var tcs1 = new TaskCompletionSource<object>(TaskCreationOptions.None);
            var tcs2 = new TaskCompletionSource<object>(TaskCreationOptions.None);
            var tcs3 = new TaskCompletionSource<object>(TaskCreationOptions.None);

            var state1 = new SimpleState(
                "state1",
                logger.StateEnterAction,
                async s => { await logger.StateExecutionAction(s); await tcs1.Task; },
                logger.StateExitAction,
                logger.StateCancelledAction);

            var state2 = new SimpleState(
                "state2",
                logger.StateEnterAction,
                async s => { await logger.StateExecutionAction(s); await tcs2.Task; },
                logger.StateExitAction,
                logger.StateCancelledAction);

            var state3 = new SimpleState(
                "state3",
                logger.StateEnterAction,
                async s => { await logger.StateExecutionAction(s); await tcs3.Task; },
                logger.StateExitAction,
                logger.StateCancelledAction);

            var parallelState = new ParallelAllState(
                "parallel",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                new StateBase[] { state1, state2, state3 });

            var task = parallelState.ExecuteAsync(CancellationToken.None);

            tcs2.TrySetResult(null);
            tcs3.TrySetResult(null);
            tcs1.TrySetResult(null);

            await task;

            var tracking = logger.ToString();

            Assert.EndsWith("<parallel;", tracking);

            var endingIndex = tracking.IndexOf("<");
            Assert.True(tracking.IndexOf("<state1;", endingIndex) > 0);
            Assert.True(tracking.IndexOf("<state2;", endingIndex) > 0);
            Assert.True(tracking.IndexOf("<state3;", endingIndex) > 0);
        }
    }
}
