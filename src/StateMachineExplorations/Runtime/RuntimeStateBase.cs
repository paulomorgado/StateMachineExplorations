namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    [DebuggerDisplay(@"\{{Name}\}")]
    public abstract class RuntimeStateBase : ITransitionTarget
    {
        private const string ExecutingExceptionMessage = "Executing!";
        private int isExecuting;

        protected RuntimeStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction)
        {
            this.Name = name;
            this.OnEnterAction = onEnterAction;
            this.OnExitAction = onExitAction;
            this.OnCanceledAction = onCanceledAction;
        }

        public string Name { get; }

        public Func<string, Task> OnEnterAction { get; }

        public Func<string, Task> OnExitAction { get; }

        public Func<string, Task> OnCanceledAction { get; }

        public async Task<RuntimeTransition> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref this.isExecuting, 1, 0) == 1)
            {
                throw new InvalidOperationException(ExecutingExceptionMessage);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var transition = await (await this.OnExecuteAsync(cancellationToken))();

                cancellationToken.ThrowIfCancellationRequested();

                return transition;
            }
            finally
            {
                Interlocked.Exchange(ref this.isExecuting, 0);
            }
        }

        protected internal virtual async Task<Func<Task<RuntimeTransition>>> OnExecuteAsync(CancellationToken cancellationToken)
        {
            await this.EnterStepAsync(cancellationToken);

            var transition = cancellationToken.IsCancellationRequested
                ? null
                : await this.ExecuteLoopAsync(cancellationToken).ConfigureAwait(false);

            return async () =>
               {
                   if (cancellationToken.IsCancellationRequested)
                   {
                       await this.CanceledStepAsync(cancellationToken).ConfigureAwait(false);
                   }
                   else
                   {
                       await this.ExitStepAsync(cancellationToken).ConfigureAwait(false);
                   }

                   return transition;
               };
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

        protected abstract Task<RuntimeTransition> ExecuteStepAsync(CancellationToken cancellationToken);

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
        protected virtual async Task CanceledStepAsync(CancellationToken cancellationToken)
        {
            if (this.OnCanceledAction != null)
            {
                try
                {
                    await this.OnCanceledAction(this.Name);
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
            if (this.isExecuting == 1)
            {
                throw new InvalidOperationException(ExecutingExceptionMessage);
            }
        }

        [DebuggerStepThrough]
        protected void EnsureExcuting()
        {
            if (this.isExecuting == 0)
            {
                throw new InvalidOperationException("Not executing!");
            }
        }

        private async Task<RuntimeTransition> ExecuteLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                RuntimeTransition transition;

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
