namespace Morgados.StateMachineExploration.Tests.Definitions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Definitions;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class IfElseStateTests
    {
        [Fact]
        public async Task IfElseState_WithExistingTransitionToStates_BuildRuntimeStateAndExecute()
        {
            var tracker = new TestTracker();

            var predicateValue = false;

            var state = new IfElseState("test")
            {
                OnEnterAction = tracker.StateEnterAction,
                OnExitAction = tracker.StateExitAction,
                OnCancelledAction = tracker.StateCancelledAction,
                Predicate = () => predicateValue,
                ElseTransition = new Transition("else")
                {
                    Action = tracker.TransitionAction,
                    Target = new StateTarget("state1"),
                },
                TrueTransition = new Transition("true")
                {
                    Action = tracker.TransitionAction,
                    Target = new StateTarget("state2"),
                },
            };

            var states = new RuntimeStateBase[]
            {
                new SimpleState("state1")
                {
                    OnEnterAction = tracker.StateEnterAction,
                    OnExecuteAction = tracker.StateExecutionAction,
                    OnExitAction = tracker.StateExitAction,
                    OnCancelledAction = tracker.StateCancelledAction,
                }.BuildRuntimeState(),
                new SimpleState("state2")
                {
                    OnEnterAction = tracker.StateEnterAction,
                    OnExecuteAction = tracker.StateExecutionAction,
                    OnExitAction = tracker.StateExitAction,
                    OnCancelledAction = tracker.StateCancelledAction,
                }.BuildRuntimeState(),
            };

            var runtimeState = state.BuildRuntimeState(states);

            await runtimeState.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;@test->state1;>state1;*state1;<state1;<test;", tracker.ToString());

            predicateValue = true;

            await runtimeState.ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;@test->state2;>state2;*state2;<state2;<test;", tracker.ToString());
        }
    }
}
