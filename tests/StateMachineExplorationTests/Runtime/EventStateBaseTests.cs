namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Morgados.StateMachine.Runtime;
    using Xunit;
    using System.Reflection;
    using FakeItEasy;

    public class EventStateBaseTests
    {
        [Fact]
        public async Task EventStateBase_TriggerWhenNotExecuting_ThrowsInvalidOperationException()
        {
            var state = new Mock<EventStateBase>("test", null, null, null).Object;

            Assert.ThrowsAsync<InvalidOperationException>(async () => await state.PublishEventAsync(null));
        }

        /*
        [Fact]
        public async Task EventStateBaseWithoutEventTransitions_WithoutCancellation_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var logger = new TestLogger();

            var stateTransition = new Transition("Targeted", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<EventStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteEventStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(stateTransition);

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());

            stateMock.Protected().Verify<Task<Transition>>("ExecuteEventStepAsync", Times.Once(), It.IsAny<CancellationToken>());
        }
        */

        [Fact]
        public async Task EventStateBaseWithoutEventTransitions_WithoutCancellation_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var logger = new TestLogger();

            var stateTransition = new Transition("Targeted", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = A.Fake<EventStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        logger.StateEnterAction,
                        logger.StateExitAction,
                        logger.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                }
            );

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .Returns(Task.FromResult<Transition>(stateTransition))
                .Once();

            var actual = await stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());
        }

        /*
        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithTriggeredEventTransition_ReturnsEventTransitionAndAcknowledgesHandledEvent()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Targeted", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);
            var stateTransition = new Transition("State", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<EventStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteEventStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(stateTransition);

            stateMock.Object.AddEventTransition(eventName, eventTransition);

            var executeTask = stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.True(await stateMock.Object.PublishEventAsync(eventName));

            var actual = await executeTask;

            Assert.Equal(eventTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());

            stateMock.Protected().Verify<Task<Transition>>("ExecuteEventStepAsync", Times.Once(), It.IsAny<CancellationToken>());
        }
        */

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithTriggeredEventTransition_ReturnsEventTransitionAndAcknowledgesHandledEvent()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Targeted", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);
            var stateTransition = new Transition("State", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = A.Fake<EventStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        logger.StateEnterAction,
                        logger.StateExitAction,
                        logger.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                }
            );

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .Returns(Task.FromResult<Transition>(null))
                .Once();

            stateMock.AddEventTransition(eventName, eventTransition);

            var executeTask = stateMock.ExecuteAsync(CancellationToken.None);

            Assert.True(await stateMock.PublishEventAsync(eventName));

            var actual = await executeTask;

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(eventTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());
        }

        /*
        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningNull_DoesntCompleteExecution()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Event", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<EventStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteEventStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(null);

            stateMock.Object.AddEventTransition(eventName, eventTransition);

            var executeTask = stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.False(executeTask.IsCompleted);
            Assert.Equal(">test;", logger.ToString());

            stateMock.Protected().Verify<Task<Transition>>("ExecuteEventStepAsync", Times.Once(), It.IsAny<CancellationToken>());
        }
        */

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningNull_DoesntCompleteExecution()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Event", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = A.Fake<EventStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        logger.StateEnterAction,
                        logger.StateExitAction,
                        logger.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                }
            );

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .Returns(Task.FromResult<Transition>(null))
                .Once();

            stateMock.AddEventTransition(eventName, eventTransition);

            var executeTask = stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.False(executeTask.IsCompleted);
            Assert.Equal(">test;", logger.ToString());
        }

        /*
        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningTargetedTransition_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Event", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);
            var stateTransition = new Transition("State", Mock.Of<TransitionTarget>(), logger.TransitionAction, null);

            var stateMock = new Mock<EventStateBase>(
                "test",
                logger.StateEnterAction,
                logger.StateExitAction,
                logger.StateCancelledAction)
            {
                CallBase = true,
            };

            stateMock.Protected()
                .Setup<Task<Transition>>("ExecuteEventStepAsync", It.IsAny<CancellationToken>())
                .ReturnsAsync(stateTransition);

            var o = stateMock.Object.GetType().GetTypeInfo().GetMethod("ExecuteEventStepAsync", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stateMock.Object, new object[] { CancellationToken.None });

            stateMock.Object.AddEventTransition(eventName, eventTransition);

            var actual = await stateMock.Object.ExecuteAsync(CancellationToken.None);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());

            stateMock.Protected().Verify<Task<Transition>>("ExecuteEventStepAsync", Times.Once(), It.IsAny<CancellationToken>());
        }
        */

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningTargetedTransition_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var logger = new TestLogger();

            var eventName = "event";

            var eventTransition = new Transition("Event", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);
            var stateTransition = new Transition("State", A.Fake<ITransitionTarget>(), logger.TransitionAction, null);

            var stateMock = A.Fake<EventStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        logger.StateEnterAction,
                        logger.StateExitAction,
                        logger.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                }
            );

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<Transition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .Returns(Task.FromResult(stateTransition))
                .Once();

            stateMock.AddEventTransition(eventName, eventTransition);

            var actual = await stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<Transition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", logger.ToString());
        }
    }
}
