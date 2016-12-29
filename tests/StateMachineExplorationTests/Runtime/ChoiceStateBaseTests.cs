namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class ChoiceStateBaseTests
    {
        [Fact]
        public async Task ChoiceStateBase_WithSelectedTransitionAndNotCancelled_ReturnsSelectedTransitionAndRunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var selectedTransition = new Transition("Selected", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);
            var elseTransition = new Transition("Else", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<ChoiceStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition)
            {
                CallBase = true,
            };
            stateMock.Protected().Setup<Transition>("SelectTransition").Returns(selectedTransition);

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(selectedTransition, actual);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithoutSelectedTransitionAndNotCancelled_ReturnElseTransitionAndRunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var elseTransition = new Transition("Else", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<ChoiceStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition)
            {
                CallBase = true,
            };

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var selectedTransition = new Transition("Selected", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);
                var elseTransition = new Transition("Else", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);

                var stateMock = new Mock<ChoiceStateBase>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction,
                    elseTransition)
                {
                    CallBase = true,
                };

                stateMock.Protected().Setup<Transition>("SelectTransition").Returns(selectedTransition);

                var actual = await stateMock.Object.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            var elseTransition = new Transition("Else", Mock.Of<ITransitionTarget>(), logger.TransitionAction, null);

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var stateMock = new Mock<ChoiceStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction,
                elseTransition)
                {
                    CallBase = true,
                };

                var actual = await stateMock.Object.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal("!test;", logger.ToString());
        }
    }
}
