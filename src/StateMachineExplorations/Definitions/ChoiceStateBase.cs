namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class ChoiceStateBase : StateBase
    {
        protected ChoiceStateBase(string name)
            : base(name)
        {
        }

        public Transition ElseTransition { get; set; }

        protected override IList<ValidationError> Validate(IList<ValidationError> errors)
        {
            if (this.ElseTransition == null)
            {
                errors.Add(new ValidationError(nameof(this.ElseTransition), $"\"{nameof(this.ElseTransition)}\" must not be null"));
            }

            return base.Validate(errors);
        }
    }
}
