namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class StateBaseTests
    {
        [Fact]
        public async Task StateBase_WithoutCancellation_RunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var state = A.Fake<StateBase>(builder =>
                builder.WithArgumentsForConstructor(new object[]
                    {
                                "test",
                                logger.StateEnterAction,
                                logger.StateExitAction,
                                logger.StateCancelledAction,
                    })
                .CallsBaseMethods());

            await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilNullTransitionAndExitActionsAndReturnsNull()
        {
            var logger = new TestLogger();

            var nonTargetedTransition = new Transition("NonTargeted", null, logger.TransitionAction, null);

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            logger.StateEnterAction,
                            logger.StateExitAction,
                            logger.StateCancelledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<Transition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, null);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(null, actual);
            Assert.Equal(">test;@test;@test;<test;", logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilTargettedTransitionAndExitActionsAndReturnsTargettedTransitionWithoutExecuting()
        {
            var logger = new TestLogger();

            var targetedTransition = new Transition("Targeted", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);
            var nonTargetedTransition = new Transition("NonTargeted", null, logger.TransitionAction, null);

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            logger.StateEnterAction,
                            logger.StateExitAction,
                            logger.StateCancelledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<Transition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, targetedTransition);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(targetedTransition, actual);
            Assert.Equal(">test;@test;@test;<test;", logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationBeforeExecution_RunsNodAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                logger.StateEnterAction,
                                logger.StateExitAction,
                                logger.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(string.Empty, logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringEnter_RunsEnterAndCancelledActions()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                new Func<string, Task>(async s => { await logger.StateEnterAction(s); cts.Cancel(); }),
                                logger.StateExitAction,
                                logger.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringExit_RunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                logger.StateEnterAction,
                                new Func<string, Task>(async s => { await logger.StateExitAction(s); cts.Cancel(); }),
                                logger.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringExecute_RunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                logger.StateEnterAction,
                                logger.StateExitAction,
                                logger.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                A.CallTo(state)
                    .Where(call => call.Method.Name == "ExecuteStepAsync")
                    .WithReturnType<Task<Transition>>()
                    .Invokes(() => cts.Cancel());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public void StateBase_ExecutingState_ThrowsInvalidOperationException()
        {
            var logger = new TestLogger();

            var tcs = new TaskCompletionSource<Transition>();

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            logger.StateEnterAction,
                            logger.StateExitAction,
                            logger.StateCancelledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<Transition>>()
                .Returns(tcs.Task);

            state.ExecuteAsync(CancellationToken.None);

            Assert.ThrowsAsync<InvalidOperationException>(() => state.ExecuteAsync(CancellationToken.None));
        }
    }
}
