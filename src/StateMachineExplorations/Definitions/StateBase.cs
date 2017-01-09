namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public abstract class StateBase
    {
        private const string NameValidationPattern = @"^[A-Za-z][\w-]*$";

        protected StateBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public Func<string, Task> OnEnterAction { get; set; }

        public Func<string, Task> OnExitAction { get; set; }

        public Func<string, Task> OnCanceledAction { get; set; }

        public bool IsValid() => !this.Validate().Any();

        public IEnumerable<ValidationError> Validate() => this.Validate(new List<ValidationError>());

        protected virtual IList<ValidationError> Validate(IList<ValidationError> errors)
        {
            Debug.Assert(errors != null, $"\"{nameof(errors)}\" must not be null.");

            if (this.Name == null)
            {
                errors.Add(new ValidationError(nameof(this.Name), $"\"{nameof(this.Name)}\" must not be null"));
            }

            if (!Regex.IsMatch(this.Name, NameValidationPattern))
            {
                errors.Add(new ValidationError(nameof(this.Name), $"\"{nameof(this.Name)}\" must conform to the \"{NameValidationPattern}\" pattern."));
            }

            return errors;
        }
    }
}
