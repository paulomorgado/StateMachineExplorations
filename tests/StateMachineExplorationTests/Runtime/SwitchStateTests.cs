namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class SwitchStateTests
    {
        [Fact]
        public async Task IfStat_WhenSelectorReturnsExistingOption_ReturnsTransitionForOption()
        {
            var logger = new TestLogger();

            var selectedTransition = new Transition("1", Mock.Of<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

            var state = new SwitchState<int>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition,
                new Dictionary<int, Transition>
                {
                    { 0, new Transition("0", null, null, null) },
                    { 1, selectedTransition },
                    { 2, new Transition("2", null, null, null) },
                },
                () => 1);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(selectedTransition, actual);
        }

        [Fact]
        public async Task IfStat_WhenSelectorReturnsNonExistingOption_ReturnsElseTransition()
        {
            var logger = new TestLogger();

            var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

            var state = new SwitchState<int>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition,
                new Dictionary<int, Transition>(),
                () => 2);

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

                var selectedTransition = new Transition("1", Mock.Of<ITransitionTarget>(), null, null);
                var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

                var state = new SwitchState<int>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    elseTransition,
                    new Dictionary<int, Transition>
                    {
                    { 0, new Transition("0", null, null, null) },
                    { 1, selectedTransition },
                    { 2, new Transition("2", null, null, null) },
                    },
                    () => 1);

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }

        [Fact]
        public async Task SwitchState_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var elseTransition = new Transition("False", Mock.Of<ITransitionTarget>(), null, null);

                var state = new SwitchState<int>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    elseTransition,
                    new Dictionary<int, Transition>(),
                    () => 2);

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }
    }
}
