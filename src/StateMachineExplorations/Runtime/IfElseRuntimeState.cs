
namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class IfElseRuntimeState : ChoiceRuntimeStateBase
    {
        private readonly Func<bool> predicate;
        private readonly RuntimeTransition trueTransition;

        public IfElseRuntimeState(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction,
            RuntimeTransition elseTransition,
            RuntimeTransition trueTransition,
            Func<bool> predicate)
            : base(name, onEnterAction, onExitAction,  onCanceledAction,elseTransition)
        {
            this.trueTransition = trueTransition;
            this.predicate = predicate;
        }

        protected override RuntimeTransition SelectTransition()
        {
            return this.predicate() ? this.trueTransition : null;
        }
    }
}
