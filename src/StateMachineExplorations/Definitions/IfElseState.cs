namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class IfElseState : ChoiceStateBase
    {
        public IfElseState(string name)
            : base(name)
        {
        }

        public Transition TrueTransition { get; set; }

        public Func<bool> Predicate { get; set; }

        protected override IList<ValidationError> Validate(IList<ValidationError> errors)
        {
            if (this.Predicate == null)
            {
                errors.Add(new ValidationError(nameof(this.Predicate), $"\"{nameof(this.Predicate)}\" must not be null"));
            }

            if (this.TrueTransition == null)
            {
                errors.Add(new ValidationError(nameof(this.TrueTransition), $"\"{nameof(this.TrueTransition)}\" must not be null"));
            }

            return base.Validate(errors);
        }
    }
}
