namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ValidationError
    {
        public ValidationError(string propertyName, string errorMessage)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(nameof(propertyName), $"{nameof(propertyName)} must not be empty.");
            }

            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException(nameof(errorMessage), $"{nameof(errorMessage)} must not be empty.");
            }

            this.PropertyName = propertyName;
            this.ErrorMessage = errorMessage;
        }

        public string PropertyName { get; }

        public string ErrorMessage { get; }
    }
}
