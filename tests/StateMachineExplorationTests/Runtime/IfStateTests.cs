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
            var logger = new TestLogger();

            var trueTransition = new Transition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfState(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition,
                trueTransition,
                () => true);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(trueTransition, actual);
        }

        [Fact]
        public async Task IfStat_WhenPredicateReturnsFalse_ReturnsElseTransition()
        {
            var logger = new TestLogger();

            var trueTransition = new Transition("True", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new IfState(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition,
                trueTransition,
                () => false);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);
        }

        [Fact]
        public async Task IfState_WhenPredicateReturnsTrueAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfState(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    new Transition("True", null, null, null),
                    new Transition("False", null, null, null),
                    () => { cts.Cancel(); return true; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public async Task IfState_WhenPredicateReturnsFalseAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = new IfState(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    new Transition("True", null, null, null),
                    new Transition("False", null, null, null),
                    () => { cts.Cancel(); return false; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }
    }
}
