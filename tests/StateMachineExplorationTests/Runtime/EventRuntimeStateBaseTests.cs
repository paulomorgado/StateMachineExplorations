namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Morgados.StateMachines.Runtime;
    using Xunit;
    using System.Reflection;
    using FakeItEasy;

    public class EventRuntimeStateBaseTests
    {
        [Fact]
        public async Task EventRuntimeStateBase_TriggerWhenNotExecuting_ThrowsInvalidOperationException()
        {
            var state = new Mock<EventRuntimeStateBase>("test", null, null, null).Object;

            Assert.ThrowsAsync<InvalidOperationException>(async () => await state.PublishEventAsync(null));
        }

        [Fact]
        public async Task EventStateBaseWithoutEventTransitions_WithoutCancellation_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var tracker = new TestTracker();

            var stateTransition = new RuntimeTransition("Targeted", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var stateMock = A.Fake<EventRuntimeStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        tracker.StateEnterAction,
                        tracker.StateExitAction,
                        tracker.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                });

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .Returns(Task.FromResult<RuntimeTransition>(stateTransition))
                .Once();

            var actual = await stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", tracker.ToString());
        }

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithTriggeredEventTransition_ReturnsEventTransitionAndAcknowledgesHandledEvent()
        {
            var tracker = new TestTracker();

            var eventName = "event";

            var eventTransition = new RuntimeTransition("Targeted", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
            var stateTransition = new RuntimeTransition("State", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var state = A.Fake<EventRuntimeStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        tracker.StateEnterAction,
                        tracker.StateExitAction,
                        tracker.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                });

            A.CallTo(state)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .Returns(Task.FromResult<RuntimeTransition>(null))
                .Once();

            state.AddEventTransition(eventName, eventTransition);

            var executeTask = state.ExecuteAsync(CancellationToken.None);

            Assert.True(await state.PublishEventAsync(eventName));

            var actual = await executeTask;

            A.CallTo(state)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(eventTransition, actual);
            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningNull_DoesntCompleteExecution()
        {
            var tracker = new TestTracker();

            var eventName = "event";

            var eventTransition = new RuntimeTransition("Event", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var stateMock = A.Fake<EventRuntimeStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        tracker.StateEnterAction,
                        tracker.StateExitAction,
                        tracker.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                });

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .Returns(Task.FromResult<RuntimeTransition>(null))
                .Once();

            stateMock.AddEventTransition(eventName, eventTransition);

            var executeTask = stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.False(executeTask.IsCompleted);
            Assert.Equal(">test;", tracker.ToString());
        }

        [Fact]
        public async Task EventStateBaseWithEventTransitions_WithoutCancellationWithoutTriggeredEventTransitionAndStateExecutionReturningTargetedTransition_ReturnsReturnValueOfExecuteEventStepAsync()
        {
            var tracker = new TestTracker();

            var eventName = "event";

            var eventTransition = new RuntimeTransition("Event", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);
            var stateTransition = new RuntimeTransition("State", A.Fake<ITransitionTarget>(), tracker.TransitionAction, null);

            var stateMock = A.Fake<EventRuntimeStateBase>(
                x =>
                {
                    x.WithArgumentsForConstructor(new object[]
                      {
                        "test",
                        tracker.StateEnterAction,
                        tracker.StateExitAction,
                        tracker.StateCancelledAction,
                      });

                    x.CallsBaseMethods();
                });

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "EnterStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExitStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .CallsBaseMethod();

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .Returns(Task.FromResult(stateTransition))
                .Once();

            stateMock.AddEventTransition(eventName, eventTransition);

            var actual = await stateMock.ExecuteAsync(CancellationToken.None);

            A.CallTo(stateMock)
                .Where(call => call.Method.Name == "ExecuteEventStepAsync")
                .WithReturnType<Task<RuntimeTransition>>()
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.Equal(stateTransition, actual);
            Assert.Equal(">test;<test;", tracker.ToString());
        }
    }
}
