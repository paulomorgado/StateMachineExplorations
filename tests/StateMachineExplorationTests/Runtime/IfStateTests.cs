namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class IfStateTests
    {
        [Fact]
        public async Task IfStat_WhenPredicateReturnsTrue_ReturnsTrueTransition()
        {
            var logger = new TestLogger();

            var trueTransition = new Transition("True", Mock.Of<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

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

            var trueTransition = new Transition("True", Mock.Of<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

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
        public async Task IfStat_WhenPredicateReturnsTrueAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var trueTransition = new Transition("True", null, null, null);
                var elseTransition = new Transition("False", null, null, null);

                var state = new IfState(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    elseTransition,
                    trueTransition,
                    () => true);

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }

        [Fact]
        public async Task IfState_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var state = new IfState(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    new Transition("True", null, null, null),
                    new Transition("False", null, null, null),
                    () => false);

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }
    }
}
