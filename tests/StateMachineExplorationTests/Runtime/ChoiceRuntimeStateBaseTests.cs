namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class ChoiceRuntimeStateBaseTests
    {
        [Fact]
        public async Task ExecuteAsync_WithSelectedTransitionAndNotCanceled_ReturnsSelectedTransitionAndRunsEnterAndExitActions()
        {
            var tracker = new TestTracker();

            var selectedTransition = new RuntimeTransition("Selected", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
            var elseTransition = new RuntimeTransition("Else", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var state = A.Fake<ChoiceRuntimeStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCanceledAction,
                            elseTransition,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "SelectTransition")
                .WithReturnType<RuntimeTransition>()
                .Returns(selectedTransition);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(selectedTransition, actual);

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithoutSelectedTransitionAndNotCanceled_ReturnElseTransitionAndRunsEnterAndExitActions()
        {
            var tracker = new TestTracker();

            var elseTransition = new RuntimeTransition("Else", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var state = A.Fake<ChoiceRuntimeStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            tracker.StateEnterAction,
                            tracker.StateExitAction,
                            tracker.StateCanceledAction,
                            elseTransition,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "SelectTransition")
                .WithReturnType<RuntimeTransition>()
                .Returns(null);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);

            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithSelectedTransitionAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var selectedTransition = new RuntimeTransition("Selected", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
                var elseTransition = new RuntimeTransition("Else", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

                var state = A.Fake<ChoiceRuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                                elseTransition,
                            })
                        .CallsBaseMethods());

                A.CallTo(state)
                    .Where(call => call.Method.Name == "SelectTransition")
                    .WithReturnType<RuntimeTransition>()
                    .ReturnsLazily(() =>
                    {
                        cts.Cancel();
                        return selectedTransition;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithoutSelectedTransitionAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<ChoiceRuntimeStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                tracker.StateEnterAction,
                                tracker.StateExitAction,
                                tracker.StateCanceledAction,
                                new RuntimeTransition("Else", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null),
                            })
                        .CallsBaseMethods());

                A.CallTo(state)
                    .Where(call => call.Method.Name == "SelectTransition")
                    .WithReturnType<RuntimeTransition>()
                    .ReturnsLazily(() =>
                    {
                        cts.Cancel();
                        return null;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
