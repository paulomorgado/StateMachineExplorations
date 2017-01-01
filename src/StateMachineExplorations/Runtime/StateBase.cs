namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay(@"\{{Name}\}")]
    public abstract class StateBase : ITransitionTarget
    {
        private Task<Transition> executingTask;

        protected StateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction)
        {
            this.Name = name;
            this.OnEnterAction = onEnterAction;
            this.OnExitAction = onExitAction;
            this.OnCancelledAction = onCancelledAction;
        }

        public string Name { get; }

        public Func<string, Task> OnEnterAction { get; }

        public Func<string, Task> OnExitAction { get; }

        public Func<string, Task> OnCancelledAction { get; }

        public async Task<Transition> ExecuteAsync(CancellationToken cancellationToken)
        {
            this.EnsureNotExcuting();

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            this.executingTask = this.EnterAndExecuteAsync(cancellationToken);

            var transition = await this.executingTask.ConfigureAwait(false);

            return await ExitOrCancelAsync(cancellationToken) ? transition : null;
        }

        internal async Task<Transition> EnterAndExecuteAsync(CancellationToken cancellationToken)
        {
            await this.EnterStepAsync(cancellationToken);

            return !cancellationToken.IsCancellationRequested
                ? await this.ExecuteLoopAsync(cancellationToken).ConfigureAwait(false)
                : null;
        }

        internal async Task<bool> ExitOrCancelAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await this.CancelledStepAsync(cancellationToken).ConfigureAwait(false);
                return false;
            }
            else
            {
                await this.ExitStepAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
        }

        [DebuggerStepThrough]
        protected virtual async Task EnterStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnEnterAction != null && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await this.OnEnterAction(this.Name);

                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        protected abstract Task<Transition> ExecuteStepAsync(CancellationToken cancellationToken);

        [DebuggerStepThrough]
        protected virtual async Task ExitStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnExitAction != null)
            {
                try
                {
                    await this.OnExitAction(this.Name);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        [DebuggerStepThrough]
        protected virtual async Task CancelledStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnCancelledAction != null)
            {
                try
                {
                    await this.OnCancelledAction(this.Name);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        [DebuggerStepThrough]
        protected void EnsureNotExcuting()
        {
            if (!(this.executingTask?.IsCompleted ?? true))
            {
                throw new InvalidOperationException("Not executing!");
            }
        }

        [DebuggerStepThrough]
        protected void EnsureExcuting()
        {
            if (this.executingTask?.IsCompleted ?? false)
            {
                throw new InvalidOperationException("Executing!");
            }
        }

        private async Task<Transition> ExecuteLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Transition transition;

                do
                {
                    transition = await this.ExecuteStepAsync(cancellationToken);

                    if (transition != null && transition.Target == null)
                    {
                        await transition.ExecuteActionAsync(cancellationToken, this.Name, this.Name);
                    }
                }
                while (!cancellationToken.IsCancellationRequested && transition != null && transition.Target == null);

                return transition;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
        }
    }
}
