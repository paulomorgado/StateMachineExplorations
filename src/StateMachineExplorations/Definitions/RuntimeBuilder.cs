namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Runtime;

    public static class RuntimeBuilder
    {
        public static RuntimeStateBase BuildRuntimeState(this StateBase state) => BuildRuntimeState(state, null);

        public static RuntimeStateBase BuildRuntimeState(this StateBase state, IEnumerable<RuntimeStateBase> runtimeStates)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var validation = state.Validate();

            if (validation.Any())
            {
                throw new ValidationException(validation);
            }

            switch (state)
            {
                case IfElseState ifElseState:
                    return new IfElseRuntimeState(
                        ifElseState.Name,
                        ifElseState.OnEnterAction,
                        ifElseState.OnExitAction,
                        ifElseState.OnCanceledAction,
                        BuildRuntimeTransition(ifElseState.ElseTransition, runtimeStates),
                        BuildRuntimeTransition(ifElseState.TrueTransition, runtimeStates),
                        ifElseState.Predicate);
                case ChoiceStateBase switchState when TryBuildSwitch(switchState, runtimeStates, out var runtimeState):
                    return runtimeState;
                case SimpleState simpleState:
                    return new SimpleRuntimeState(
                        simpleState.Name,
                        simpleState.OnEnterAction,
                        simpleState.OnExecuteAction,
                        simpleState.OnExitAction,
                        simpleState.OnCanceledAction);
                case CompositeState compositeState:
                    var subStates = compositeState.SubStates.Select(s => BuildRuntimeState(s)).ToList();
                    var compositeRuntimeState = new CompositeRuntimeState(
                        compositeState.Name,
                        compositeState.OnEnterAction,
                        compositeState.OnExitAction,
                        compositeState.OnCanceledAction,
                        BuildRuntimeTransition(compositeState.InitialTransition, subStates));
                    foreach (var pair in compositeState.Transitions)
                    {
                        foreach (var transition in pair.Value)
                        {
                            compositeRuntimeState.AddEventTransition(pair.Key, BuildRuntimeTransition(transition, subStates));
                        }
                    }

                    return compositeRuntimeState;
                case ParallelState parallelState when parallelState.Mode == ParallelModes.All:
                    return new ParallelAllRuntimeState(
                        parallelState.Name,
                        parallelState.OnEnterAction,
                        parallelState.OnExitAction,
                        parallelState.OnCanceledAction,
                        parallelState.Regions.Select(r => BuildRuntimeState(r)));
                case ParallelState parallelState when parallelState.Mode == ParallelModes.Any:
                    return new ParallelAnyRuntimeState(
                        parallelState.Name,
                        parallelState.OnEnterAction,
                        parallelState.OnExitAction,
                        parallelState.OnCanceledAction,
                        parallelState.Regions.Select(r => BuildRuntimeState(r)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), $"Invalid state type: {state.GetType().FullName}");
            }
        }

        private static bool TryBuildSwitch(ChoiceStateBase state, IEnumerable<RuntimeStateBase> runtimeStates, out ChoiceRuntimeStateBase runtimeState)
        {
            runtimeState = null;

            var stateType = state.GetType();

            if (!(stateType.IsConstructedGenericType && stateType.GetGenericTypeDefinition() != typeof(SwitchState<>)))
            {
                return false;
            }

            runtimeState = typeof(RuntimeBuilder)
                .GetTypeInfo()
                .MakeGenericType(state.GetType().GenericTypeArguments)
                .GetRuntimeMethods()
                .Single(mi => mi.Name == nameof(BuildRuntimeSwitchState))
                .Invoke(null, new object[] { state, runtimeStates })
                as ChoiceRuntimeStateBase
            ;

            return runtimeState is ChoiceRuntimeStateBase;
        }

        private static RuntimeTransition BuildRuntimeTransition(TransitionBase transitionBase, IEnumerable<RuntimeStateBase> runtimeStates)
        {
            switch (transitionBase)
            {
                case Transition transition:
                    return BuildRuntimeTransition(runtimeStates, transition);
                case InternalTransition transition:
                    return BuildInternalRuntimeTransition(transition);
                case ExternalTransition transition:
                    return BuildExternalRuntimeTransition(runtimeStates, transition);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(transitionBase),
                        $"Invalid transition type: {transitionBase.GetType().FullName}");
            }
        }

        private static RuntimeTransition BuildExternalRuntimeTransition(IEnumerable<RuntimeStateBase> runtimeStates, ExternalTransition transition)
        {
            return new RuntimeTransition(
                transition.Name,
                runtimeStates?.Single(s => s.Name == transition.Target.Name),
                transition.Action,
                transition.Guard);
        }

        private static RuntimeTransition BuildInternalRuntimeTransition(InternalTransition transition)
        {
            return new RuntimeTransition(
                transition.Name,
                null,
                transition.Action,
                transition.Guard);
        }

        private static RuntimeTransition BuildRuntimeTransition(IEnumerable<RuntimeStateBase> runtimeStates, Transition transition)
        {
            return new RuntimeTransition(
                transition.Name,
                runtimeStates?.Single(s => s.Name == transition.Target.Name),
                transition.Action,
                null);
        }

        private static SwitchRuntimeState<T> BuildRuntimeSwitchState<T>(SwitchState<T> switchState, IEnumerable<RuntimeStateBase> runtimeStates)
        {
            var selectionTransitions = new Dictionary<T, RuntimeTransition>();

            if (switchState.SelectionTransitions != null)
            {
                foreach (var pair in switchState.SelectionTransitions)
                {
                    selectionTransitions.Add(pair.Key, BuildRuntimeTransition(pair.Value, runtimeStates));
                }
            }

            return new SwitchRuntimeState<T>(
                switchState.Name,
                switchState.OnEnterAction,
                switchState.OnExitAction,
                switchState.OnCanceledAction,
                BuildRuntimeTransition(switchState.ElseTransition, runtimeStates),
                selectionTransitions,
                switchState.Selector);
        }
    }
}
