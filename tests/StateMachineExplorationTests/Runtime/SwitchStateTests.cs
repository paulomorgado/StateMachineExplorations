namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Moq;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class SwitchStateTests
    {
        [Fact]
        public async Task SwitchState_WhenSelectorReturnsExistingOption_ReturnsTransitionForOption()
        {
            var logger = new TestLogger();

            var selectedTransition = new Transition("1", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

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
        public async Task SwitchState_WhenSelectorReturnsNonExistingOption_ReturnsElseTransition()
        {
            var logger = new TestLogger();

            var elseTransition = new Transition("False", A.Fake<ITransitionTarget>(), null, null);

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
        public async Task SwitchState_WithSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchState<int>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    new Transition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, Transition>
                    {
                        { 0, new Transition("0", null, null, null) },
                        { 1, new Transition("1", A.Fake<ITransitionTarget>(), null, null)},
                        { 2, new Transition("2", null, null, null) },
                    },
                    () => { cts.Cancel(); return 1; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public async Task SwitchState_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchState<int>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    new Transition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, Transition>(),
                    () => { cts.Cancel(); return 2; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }
    }
}
