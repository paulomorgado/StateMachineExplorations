﻿namespace Morgados.StateMachineExploration.Tests.Runtime
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
            var tracker = new TestTracker();

            var state = A.Fake<StateBase>(builder =>
                builder.WithArgumentsForConstructor(new object[]
                    {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCancelledAction,
                    })
                .CallsBaseMethods());

            await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilNullTransitionAndExitActionsAndReturnsNull()
        {
            var tracker = new TestTracker();

            var nonTargetedTransition = new Transition("NonTargeted", null, tracker.TransitionAction, null);

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCancelledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<Transition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, null);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(null, actual);
            Assert.Equal(">test;@test;@test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithoutCancellationAndNonTargettedTransitions_RunsEnterAndExecutesTransitionsUntilTargettedTransitionAndExitActionsAndReturnsTargettedTransitionWithoutExecuting()
        {
            var tracker = new TestTracker();

            var targetedTransition = new Transition("Targeted", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
            var nonTargetedTransition = new Transition("NonTargeted", null, tracker.TransitionAction, null);

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCancelledAction,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteStepAsync")
                .WithReturnType<Task<Transition>>()
                .ReturnsNextFromSequence(nonTargetedTransition, nonTargetedTransition, targetedTransition);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(targetedTransition, actual);
            Assert.Equal(">test;@test;@test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationBeforeExecution_RunsNodAction()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(string.Empty, tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringEnter_RunsEnterAndCancelledActions()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                new Func<string, Task>(async s => { await tracker.StateEnterAction(s); cts.Cancel(); }),
                                tracker.StateExitAction,
                                tracker.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringExit_RunsEnterAndExitActions()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                new Func<string, Task>(async s => { await tracker.StateExitAction(s); cts.Cancel(); }),
                                tracker.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task StateBase_WithCancellationDuringExecute_RunsEnterAndExitActions()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<StateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCancelledAction,
                            })
                        .CallsBaseMethods());

                A.CallTo(state)
                    .Where(call => call.Method.Name == "ExecuteStepAsync")
                    .WithReturnType<Task<Transition>>()
                    .Invokes(() => cts.Cancel());

                await state.ExecuteAsync(cts.Token);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public void StateBase_ExecutingState_ThrowsInvalidOperationException()
        {
            var tracker = new TestTracker();

            var tcs = new TaskCompletionSource<Transition>();

            var state = A.Fake<StateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCancelledAction,
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
