namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class SwitchState<T> : ChoiceStateBase
    {
        protected SwitchState(string name)
            : base(name)
        {
        }

        public IDictionary<T, Transition> SelectionTransitions { get; } = new Dictionary<T, Transition>();

        public Func<T> Selector { get; set; }

        protected override IList<ValidationError> Validate(IList<ValidationError> errors)
        {
            if (this.Selector == null)
            {
                errors.Add(new ValidationError(nameof(this.Selector), $"\"{nameof(this.Selector)}\" must not be null"));
            }

            foreach (var pair in this.SelectionTransitions)
            {
                if (pair.Value == null)
                {
                    errors.Add(new ValidationError(nameof(this.SelectionTransitions), $"The transition for \"{pair.Key}\" must not be null"));
                }
            }

            return base.Validate(errors);
        }
    }
}
