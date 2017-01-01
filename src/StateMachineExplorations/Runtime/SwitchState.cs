
namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SwitchState<TSwitch> : ChoiceStateBase
    {
        private readonly Func<TSwitch> selector;
        private readonly IDictionary<TSwitch, Transition> selectionTransitions;

        public SwitchState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            Transition elseTransition,
            IDictionary<TSwitch, Transition> selectionTransitions,
            Func<TSwitch> selector)
            : base(name, onEnterAction, onExitAction, onCancelledAction, elseTransition)
        {
            this.selectionTransitions = selectionTransitions;
            this.selector = selector;
        }

        protected override Transition SelectTransition()
        {
            this.selectionTransitions.TryGetValue(this.selector(), out var transition);

            return transition;
        }
    }
}
