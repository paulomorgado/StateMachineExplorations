namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class StateBaseTests
    {
        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithoutCancellation_RunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var stateMock = new Mock<StateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilNullTransitionAndExitActionsAndReturnsNull()
        {
            var logger = new TestLogger();

            var nonTargetedTransition = new Transition("NonTargeted", null, logger.TransitionAction, null);

            var stateMock = new Mock<StateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(nonTargetedTransition)
                .Callback(() => stateMock.Protected()
                    .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                    .ReturnsAsync(nonTargetedTransition)
                    .Callback(() => stateMock.Protected()
                        .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                        .ReturnsAsync(null)
                        .Verifiable())
                    .Verifiable())
                .Verifiable();

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(null, actual);
            Assert.Equal(">test;@test;@test;<test;", logger.ToString());

            stateMock.VerifyAll();
        }

        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilTargettedTransitionAndExitActionsAndReturnsTargettedTransitionWithoutExecuting()
        {
            var logger = new TestLogger();

            var targetedTransition = new Transition("Targeted", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);
            var nonTargetedTransition = new Transition("NonTargeted", null, logger.TransitionAction, null);

            var stateMock = new Mock<StateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(nonTargetedTransition)
                .Callback(() => stateMock.Protected()
                    .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                    .ReturnsAsync(nonTargetedTransition)
                    .Callback(() => stateMock.Protected()
                        .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                        .ReturnsAsync(targetedTransition)
                        .Verifiable())
                    .Verifiable())
                .Verifiable();

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(targetedTransition, actual);
            Assert.Equal(">test;@test;@test;<test;", logger.ToString());

            stateMock.VerifyAll();
        }

        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithCancellationBeforeExecution_RunsOnlyCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var stateMock = new Mock<StateBase>(
                    "test",
                    logger.StateEnterAction,
                    logger.StateExitAction,
                    logger.StateCancelledAction)
                {
                    CallBase = true,
                };

                await stateMock.Object.ExecuteAsync(cts.Token);
            }

            Assert.Equal("!test;", logger.ToString());
        }

        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithCancellationDuringEnter_RunsEnterAndCancelledActions()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var stateMock = new Mock<StateBase>(
                    "test",
                    new Func<string, Task>(async s => { await logger.StateEnterAction(s); cts.Cancel(); }),
                    logger.StateExitAction,
                    logger.StateCancelledAction)
                {
                    CallBase = true,
                };

                await stateMock.Object.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public async Task StateBaseWithoutEventTransitions_WithCancellationDuringExit_RunsEnterAndExitAndCancelledActions()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var stateMock = new Mock<StateBase>(
                    "test",
                    logger.StateEnterAction,
                    new Func<string, Task>(async s => { await logger.StateExitAction(s); cts.Cancel(); }),
                    logger.StateCancelledAction)
                {
                    CallBase = true,
                };

                await stateMock.Object.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;<test;!test;", logger.ToString());
        }

        [Fact]
        public void StateBaseWithoutEventTransitions_ExecutingState_ThrowsInvalidOperationException()
        {
            var logger = new TestLogger();

            var tcs = new TaskCompletionSource<Transition>();

            var stateMock = new Mock<StateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };
            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteStepAsync", It.IsAny<CancellationToken>())
                .Returns(tcs.Task)
                .Verifiable();

            stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.ThrowsAsync<InvalidOperationException>(() => stateMock.Object.ExecuteAsync(CancellationToken.None));

            stateMock.VerifyAll();
        }
    }
}
