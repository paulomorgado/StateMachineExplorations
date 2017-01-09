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
                OnCanceledAction = tracker.StateCanceledAction,
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
                    OnCanceledAction = tracker.StateCanceledAction,
                }.BuildRuntimeState(),
                new SimpleState("state2")
                {
                    OnEnterAction = tracker.StateEnterAction,
                    OnExecuteAction = tracker.StateExecutionAction,
                    OnExitAction = tracker.StateExitAction,
                    OnCanceledAction = tracker.StateCanceledAction,
                }.BuildRuntimeState(),
            };

            var runtimeState = state.BuildRuntimeState(states);

            var runtimeTransition = await runtimeState.ExecuteAsync(CancellationToken.None);

            Assert.Equal(state.ElseTransition.Target.Name, runtimeTransition.Target.Name);
            Assert.Equal(">test;<test;", tracker.ToString());

            predicateValue = true;

            tracker.Clear();

            runtimeTransition = await runtimeState.ExecuteAsync(CancellationToken.None);

            Assert.Equal(state.TrueTransition.Target.Name, runtimeTransition.Target.Name);
            Assert.Equal(">test;<test;", tracker.ToString());
        }
    }
}
