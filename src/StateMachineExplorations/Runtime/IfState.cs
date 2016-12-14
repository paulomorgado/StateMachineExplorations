
namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class IfState : ChoiceStateBase
    {
        private readonly Func<bool> predicate;
        private readonly Transition trueTransition;

        public IfState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction,
            Transition elseTransition,
            Transition trueTransition,
            Func<bool> predicate)
            : base(name, onEnterAction, onExitAction,  onCancelledAction,elseTransition)
        {
            this.trueTransition = trueTransition;
            this.predicate = predicate;
        }

        protected override Transition SelectTransition()
        {
            return this.predicate() ? this.trueTransition : null;
        }
    }
}
