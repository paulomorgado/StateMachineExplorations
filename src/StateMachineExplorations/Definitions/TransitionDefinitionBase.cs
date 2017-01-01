﻿namespace Morgados.StateMachine.Definitions
{
    using System;
    using System.Threading.Tasks;

    public abstract class TransitionDefinitionBase
    {
        protected TransitionDefinitionBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public Func<string, Task> Action { get; set; }
    }
}
