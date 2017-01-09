namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class RuntimeStateBaseTests
    {
        [Fact]
        public async Task ExecuteAsync_WithoutCancellation_RunsEnterAndExitActions()
        {
            var tracker = new TestTracker();

            var state = A.Fake<RuntimeStateBase>(builder =>
                builder.WithArgumentsForConstructor(new object[]
                    {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                    })
                .CallsBaseMethods());

            await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilNullTransitionAndExitActionsAndReturnsNull()
        {
            var tracker = new TestTracker();

            var nonTargetedTransition = new RuntimeTransition("NonTargeted", null, tracker.TransitionAction, null);

            var state = A.Fake<RuntimeStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCanceledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, null);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(null, actual);
            Assert.Equal(">test;@test;@test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilTargettedTransitionAndExitActionsAndReturnsTargettedTransitionWithoutExecuting()
        {
            var tracker = new TestTracker();

            var targetedTransition = new RuntimeTransition("Targeted", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
            var nonTargetedTransition = new RuntimeTransition("NonTargeted", null, tracker.TransitionAction, null);

            var state = A.Fake<RuntimeStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCanceledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, targetedTransition);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(targetedTransition, actual);
            Assert.Equal(">test;@test;@test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_CanceledBeforeExecution_RunsNodActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var state = A.Fake<RuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                            })
                        .CallsBaseMethods());

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(string.Empty, tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_CanceledDuringEnter_RunsEnterAndCanceledActionsAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<RuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                new Func<string, Task>(async s =>
                                {
                                    await tracker.StateEnterAction(s);
                                    cts.Cancel();
                                }),
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                            })
                        .CallsBaseMethods());

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_CanceledDuringExit_RunsEnterAndExitActionsAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<RuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                new Func<string, Task>(async s =>
                                {
                                    await tracker.StateExitAction(s);
                                    cts.Cancel();
                                }),
                                tracker.StateCanceledAction,
                            })
                        .CallsBaseMethods());

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithCancellationDuringExecute_RunsEnterAndExitActionsAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<RuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                            })
                        .CallsBaseMethods());

                A.CallTo(state)
                    .Where(call => call.Method.Name == "ExecuteStepAsync")
                    .WithReturnType<Task<RuntimeTransition>>()
                    .Invokes(() => cts.Cancel());

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_ExecutingState_ThrowsInvalidOperationExceptionAsync()
        {
            var tracker = new TestTracker();

            var tcs = new TaskCompletionSource<RuntimeTransition>();

            var state = A.Fake<RuntimeStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCanceledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .Returns(tcs.Task);

            var task = state.ExecuteAsync(CancellationToken.None);

            await Assert.ThrowsAsync<InvalidOperationException>(() => state.ExecuteAsync(CancellationToken.None));

            tcs.SetResult(null);

            await task;
        }
    }
}
