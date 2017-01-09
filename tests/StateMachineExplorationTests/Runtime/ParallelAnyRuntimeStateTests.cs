namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using Morgados.StateMachines.Runtime;
    using Xunit;
    using System.Threading.Tasks;

    public class ParallelAnyRuntimeStateTests
    {
        [Fact]
        public async Task ParallelAnyRuntimeState_WithoutCancellationAndEventTransitions_ExitsOnlyOneStateCancelsRest()
        {
            var tracker = new TestTracker();

            var tcs1 = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs2 = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs3 = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            var state1 = new SimpleRuntimeState(
                "state1",
                tracker.StateEnterAction,
                async s =>
                {
                    await tracker.StateExecutionAction(s);
                    await tcs1.Task;
                },
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var state2 = new SimpleRuntimeState(
                "state2",
                tracker.StateEnterAction,
                async s =>
                {
                    await tracker.StateExecutionAction(s);
                    await tcs2.Task;
                },
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var state3 = new SimpleRuntimeState(
                "state3",
                tracker.StateEnterAction,
                async s =>
                {
                    await tracker.StateExecutionAction(s);
                    await tcs3.Task;
                },
                tracker.StateExitAction,
                tracker.StateCanceledAction);

            var parallelState = new ParallelAnyRuntimeState(
                "parallel",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                new RuntimeStateBase[] { state1, state2, state3 });

            var task = parallelState.ExecuteAsync(CancellationToken.None);

            tcs2.TrySetResult(null);
            tcs3.TrySetResult(null);
            tcs1.TrySetResult(null);

            await task;

            var tracking = tracker.ToString();

            Assert.EndsWith("<parallel;", tracking);

            var endingIndex = tracking.IndexOf("<");
            Assert.True(tracking.IndexOf("!state1;", endingIndex) > 0);
            Assert.True(tracking.IndexOf("<state2;", endingIndex) > 0);
            Assert.True(tracking.IndexOf("!state3;", endingIndex) > 0);
        }
    }
}
