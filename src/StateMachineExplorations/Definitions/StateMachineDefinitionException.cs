namespace Morgados.StateMachines.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class StateMachineDefinitionException : StateMachineException
    {
        public StateMachineDefinitionException()
        {
        }

        public StateMachineDefinitionException(string message)
            : base(message)
        {
        }

        public StateMachineDefinitionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
