namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class IfElseRuntimeStateTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenPredicateReturnsTrue_ReturnsTrueTransition()
        {
            var tracker = new TestTracker();

            var trueTransition = new RuntimeTransition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfElseRuntimeState(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                elseTransition,
                trueTransition,
                () => true);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(trueTransition, actual);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPredicateReturnsFalse_ReturnsElseTransition()
        {
            var tracker = new TestTracker();

            var trueTransition = new RuntimeTransition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfElseRuntimeState(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                elseTransition,
                trueTransition,
                () => false);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPredicateReturnsTrueAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfElseRuntimeState(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCanceledAction,
                    new RuntimeTransition("True", null, null, null),
                    new RuntimeTransition("False", null, null, null),
                    () =>
                    {
                        cts.Cancel();
                        return true;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WhenPredicateReturnsFalseAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfElseRuntimeState(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCanceledAction,
                    new RuntimeTransition("True", null, null, null),
                    new RuntimeTransition("False", null, null, null),
                    () =>
                    {
                        cts.Cancel();
                        return false;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
