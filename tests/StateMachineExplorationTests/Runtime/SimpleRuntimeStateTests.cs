namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class SimpleRuntimeStateTests
    {
        [Fact]
        public async Task SimpleRuntimeState_WithoutTransitions_CompletesImmediatelly()
        {
            var tracker = new TestTracker();

            var state = new SimpleRuntimeState(
                "test",
                tracker.StateEnterAction,
                tracker.StateExecutionAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction);

            await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;*test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task SimpleRuntimeState_WithCancellationWithoutTransitions_CompletesImmediatellyAndRunsCancelledActionAndThrows()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();

            var tracker = new TestTracker();

            var state = new SimpleRuntimeState(
                "test",
                tracker.StateEnterAction,
                async s => { await tracker.StateExecutionAction(s); await tcs.Task; },
                tracker.StateExitAction,
                tracker.StateCancelledAction);

            Task<RuntimeTransition> task = state.ExecuteAsync(cts.Token);

            cts.Cancel();
            tcs.SetResult(null);

            Assert.ThrowsAsync<OperationCanceledException>(async () => await task);

            Assert.Equal(">test;*test;!test;", tracker.ToString());
        }
    }
}
