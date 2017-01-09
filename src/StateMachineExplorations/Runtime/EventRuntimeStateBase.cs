namespace Morgados.StateMachines.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class EventRuntimeStateBase : RuntimeStateBase
    {
        private readonly Dictionary<string, LinkedList<RuntimeTransition>> transitions = new Dictionary<string, LinkedList<RuntimeTransition>>();
        private EventChannel<RuntimeTransition> eventChannel;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected EventRuntimeStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCanceledAction)
            : base(name, onEnterAction, onExitAction, onCanceledAction)
        {
        }

        public async Task<bool?> PublishEventAsync(string eventName)
        {
            this.EnsureExcuting();

            return await this.OnPublishEventAsync(eventName);
        }

        public void AddEventTransition(string eventName, RuntimeTransition transition)
        {
            this.EnsureNotExcuting();

            if (!this.transitions.TryGetValue(eventName, out var transitionList))
            {
                transitionList = new LinkedList<RuntimeTransition>();
                this.transitions.Add(eventName, transitionList);
            }

            transitionList.AddLast(transition);
        }

        protected internal virtual async Task<bool?> OnPublishEventAsync(string eventName)
        {
            var eventChannel = this.eventChannel;

            if (eventChannel == null)
            {
                return null;
            }

            if (this.transitions.TryGetValue(eventName, out var transitionList))
            {
                foreach (var transition in transitionList)
                {
                    if (transition.Guard(this.Name))
                    {
                        return await eventChannel.SendAsync(transition);
                    }
                }
            }

            return false;
        }

        protected internal override async Task<Func<Task<RuntimeTransition>>> OnExecuteAsync(CancellationToken cancellationToken)
        {
            if (this.transitions.Count == 0)
            {
                return await base.OnExecuteAsync(cancellationToken).ConfigureAwait(false);
            }

            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                CancellationToken.None);

            var currentCancellationToken = this.cancellationTokenSource.Token;

            Interlocked.Exchange(ref this.eventChannel, new EventChannel<RuntimeTransition>());

            try
            {
                return await base.OnExecuteAsync(currentCancellationToken).ConfigureAwait(false);
            }
            finally
            {
                var eventChannel = this.eventChannel;
                Interlocked.Exchange(ref this.eventChannel, null);
                eventChannel?.Acknowledge(null);

                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }
        }

        protected override sealed async Task<RuntimeTransition> ExecuteStepAsync(CancellationToken cancellationToken)
        {
            if (this.transitions.Count == 0)
            {
                return await this.ExecuteEventStepAsync(cancellationToken).ConfigureAwait(false);
            }

            return await this.ExecuteStepWithTransitionsAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<RuntimeTransition> ExecuteStepWithTransitionsAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(this.eventChannel != null);
            Debug.Assert(this.cancellationTokenSource != null);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            try
            {
                var stateTask = this.ExecuteEventStepAsync(cancellationToken);
                var eventTask = this.eventChannel.ReceiveAsync();
                var transitionTask = await Task.WhenAny(eventTask, stateTask);

                if (transitionTask == stateTask)
                {
                    var transition = await stateTask.ConfigureAwait(false);

                    if (transition != null)
                    {
                        return transition;
                    }
                }

                {
                    var transition = await eventTask.ConfigureAwait(false);
                    this.eventChannel.Acknowledge(true);
                    this.cancellationTokenSource.Cancel();
                    return transition;
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                return null;
            }
            catch (Exception)
            {
                // TODO: Error handling.
                return null;
            }
        }

        protected abstract Task<RuntimeTransition> ExecuteEventStepAsync(CancellationToken cancellationToken);
    }
}