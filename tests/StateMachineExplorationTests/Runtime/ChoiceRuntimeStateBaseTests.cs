namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class ChoiceRuntimeStateBaseTests
    {
        [Fact]
        public async Task ChoiceRuntimeStateBase_WithSelectedTransitionAndNotCancelled_ReturnsSelectedTransitionAndRunsEnterAndExitActions()
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
                            tracker.StateCancelledAction,
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
        public async Task ChoiceRuntimeStateBase_WithoutSelectedTransitionAndNotCancelled_ReturnElseTransitionAndRunsEnterAndExitActions()
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
                            tracker.StateCancelledAction,
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
        public async Task ChoiceRuntimeStateBase_WithSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
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
                                tracker.StateCancelledAction,
                                elseTransition,
                            })
                        .CallsBaseMethods());

                // TODO: FakeItEasy bug
                //A.CallTo(state)
                //    .Where(call => call.Method.Name == "ExecuteStepAsync")
                //    .WithReturnType<Task<Transition>>()
                //    .Invokes(() => cts.Cancel())
                //    .CallsBaseMethod();

                A.CallTo(state)
                    .Where(call => call.Method.Name == "SelectTransition")
                    .WithReturnType<RuntimeTransition>()
                    // TODO: FakeItEasy bug
                    //.Returns(selectedTransition);
                    .ReturnsLazily(() => { cts.Cancel(); return selectedTransition; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ChoiceRuntimeStateBase_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
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
                                tracker.StateCancelledAction,
                                new RuntimeTransition("Else", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null),
                            })
                        .CallsBaseMethods());

                // TODO: FakeItEasy bug
                //A.CallTo(state)
                //    .Where(call => call.Method.Name == "ExecuteStepAsync")
                //    .WithReturnType<Task<Transition>>()
                //    .Invokes(() => cts.Cancel())
                //    .CallsBaseMethod();

                // TODO: FakeItEasy bug
                A.CallTo(state)
                    .Where(call => call.Method.Name == "SelectTransition")
                    .WithReturnType<RuntimeTransition>()
                    .ReturnsLazily(() => { cts.Cancel(); return (RuntimeTransition)null; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
