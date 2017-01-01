namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class ChoiceStateBaseTests
    {
        [Fact]
        public async Task ChoiceStateBase_WithSelectedTransitionAndNotCancelled_ReturnsSelectedTransitionAndRunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var selectedTransition = new Transition("Selected", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);
            var elseTransition = new Transition("Else", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var state = A.Fake<ChoiceStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            logger.StateEnterAction,
                            logger.StateExitAction,
                            logger.StateCancelledAction,
                            elseTransition,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "SelectTransition")
                .WithReturnType<Transition>()
                .Returns(selectedTransition);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(selectedTransition, actual);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithoutSelectedTransitionAndNotCancelled_ReturnElseTransitionAndRunsEnterAndExitActions()
        {
            var logger = new TestLogger();

            var elseTransition = new Transition("Else", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var state = A.Fake<ChoiceStateBase>(builder =>
                builder
                    .WithArgumentsForConstructor(new object[]
                        {
                            "test",
                            logger.StateEnterAction,
                            logger.StateExitAction,
                            logger.StateCancelledAction,
                            elseTransition,
                        })
                    .CallsBaseMethods());

            A.CallTo(state)
                .Where(call => call.Method.Name == "SelectTransition")
                .WithReturnType<Transition>()
                .Returns(null);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);

            Assert.Equal(">test;<test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var selectedTransition = new Transition("Selected", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);
                var elseTransition = new Transition("Else", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

                var state = A.Fake<ChoiceStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                logger.StateEnterAction,
                                logger.StateExitAction,
                                logger.StateCancelledAction,
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
                    .WithReturnType<Transition>()
                    // TODO: FakeItEasy bug
                    //.Returns(selectedTransition);
                    .ReturnsLazily(() => { cts.Cancel(); return selectedTransition; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }

        [Fact]
        public async Task ChoiceStateBase_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var logger = new TestLogger();

            using (var cts = new CancellationTokenSource())
            {
                var state = A.Fake<ChoiceStateBase>(builder =>
                    builder
                        .WithArgumentsForConstructor(new object[]
                            {
                                "test",
                                logger.StateEnterAction,
                                logger.StateExitAction,
                                logger.StateCancelledAction,
                                new Transition("Else", A.Fake<ITransitionTarget>(), logger.TransitionAction, null),
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
                    .WithReturnType<Transition>()
                    .ReturnsLazily(() => { cts.Cancel(); return (Transition)null; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", logger.ToString());
        }
    }
}
