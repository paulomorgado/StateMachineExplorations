
namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SwitchRuntimeState<TSwitch> : ChoiceRuntimeStateBase
    {
        private readonly Func<TSwitch> selector;
        private readonly IDictionary<TSwitch, RuntimeTransition> selectionTransitions;

        public SwitchRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction,
            RuntimeTransition elseTransition,
            IDictionary<TSwitch, RuntimeTransition> selectionTransitions,
            Func<TSwitch> selector)
            : base(name, onEnterAction, onExitAction, onCanceledAction, elseTransition)
        {
            this.selectionTransitions = selectionTransitions;
            this.selector = selector;
        }

        protected override RuntimeTransition SelectTransition()
        {
            this.selectionTransitions.TryGetValue(this.selector(), out var transition);

            return transition;
        }
    }
}
