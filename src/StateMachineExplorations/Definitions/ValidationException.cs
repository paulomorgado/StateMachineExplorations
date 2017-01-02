namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ValidationException : StateMachineDefinitionException
    {
        public ValidationException(IEnumerable<ValidationError> errors)
            : base(GetMessage(errors))
        {
            this.Errors = errors;
        }

        public ValidationException(IEnumerable<ValidationError> errors, Exception innerException)
            : base(GetMessage(errors), innerException)
        {
            this.Errors = errors;
        }

        public IEnumerable<ValidationError> Errors { get; }

        private static string GetMessage(IEnumerable<ValidationError> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            return errors.FirstOrDefault()?.ErrorMessage ?? "Errors found in validation.";
        }
    }
}
