namespace Morgados.StateMachines
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StateMachineException : Exception
    {
        public StateMachineException()
        {
        }

        public StateMachineException(string message)
            : base(message)
        {
        }

        public StateMachineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
