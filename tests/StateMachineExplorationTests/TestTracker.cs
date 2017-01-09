namespace Morgados.StateMachineExploration.Tests
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay(@"\{{ToString()}\}")]
    internal class TestTracker
    {
        private readonly StringBuilder builder;

        public TestTracker()
        {
            this.builder = new StringBuilder();

            this.StateEnterAction = s =>
            {
                this.builder
                    .Append(">")
                    .Append(s)
                    .Append(";");

                return Task.CompletedTask;
            };

            this.StateCanceledAction = s =>
            {
                this.builder
                    .Append("!")
                    .Append(s)
                    .Append(";");

                return Task.CompletedTask;
            };

            this.StateExecutionAction = s =>
            {
                this.builder
                    .Append("*")
                    .Append(s)
                    .Append(";");

                return Task.CompletedTask;
            };

            this.StateExitAction = s =>
            {
                this.builder
                    .Append("<")
                    .Append(s)
                    .Append(";");

                return Task.CompletedTask;
            };

            this.TransitionAction = (cts, s, t) =>
            {
                this.builder.Append("@").Append(s);

                if (s != t)
                {
                    this.builder.Append("->").Append(t);
                }

                this.builder.Append(";");

                return Task.CompletedTask;
            };
        }

        public Func<string, Task> StateCanceledAction { get; }

        public Func<string, Task> StateEnterAction { get; }

        public Func<string, Task> StateExecutionAction { get; }

        public Func<string, Task> StateExitAction { get; }

        public Func<CancellationToken, string, string, Task> TransitionAction { get; }

        public void Clear() => this.builder.Clear();

        public Task LogAsync(string text)
        {
            this.builder.Append(text);

            return Task.CompletedTask;
        }

        public override string ToString() => this.builder.ToString();
    }
}
