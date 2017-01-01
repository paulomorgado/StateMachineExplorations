namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class IfStateTests
    {
        [Fact]
        public async Task IfStat_WhenPredicateReturnsTrue_ReturnsTrueTransition()
        {
            var tracker = new TestTracker();

            var trueTransition = new Transition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfState(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction,
                elseTransition,
                trueTransition,
                () => true);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(trueTransition, actual);
        }

        [Fact]
        public async Task IfStat_WhenPredicateReturnsFalse_ReturnsElseTransition()
        {
            var tracker = new TestTracker();

            var trueTransition = new Transition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfState(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction,
                elseTransition,
                trueTransition,
                () => false);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);
        }

        [Fact]
        public async Task IfState_WhenPredicateReturnsTrueAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfState(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCancelledAction,
                    new Transition("True", null, null, null),
                    new Transition("False", null, null, null),
                    () => { cts.Cancel(); return true; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task IfState_WhenPredicateReturnsFalseAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfState(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCancelledAction,
                    new Transition("True", null, null, null),
                    new Transition("False", null, null, null),
                    () => { cts.Cancel(); return false; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
