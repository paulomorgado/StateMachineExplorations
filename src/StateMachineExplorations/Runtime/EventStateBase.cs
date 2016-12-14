namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class EventStateBase : StateBase
    {
        private readonly Dictionary<string, LinkedList<Transition>> transitions = new Dictionary<string, LinkedList<Transition>>();
        private EventChannel<Transition> eventChannel;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected EventStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
        }

        public async Task<bool> PublishEventAsync(string eventName)
        {
            this.EnsureExcuting();

            return await this.OnPublishEventAsync(eventName);
        }

        public void AddEventTransition(string eventName, Transition transition)
        {
            this.EnsureNotExcuting();

            if (!this.transitions.TryGetValue(eventName, out var transitionList))
            {
                transitionList = new LinkedList<Transition>();
                this.transitions.Add(eventName, transitionList);
            }

            transitionList.AddLast(transition);
        }

        protected internal virtual async Task<bool> OnPublishEventAsync(string eventName)
        {
            var eventChannel = this.eventChannel;

            if (eventChannel == null)
            {
                return false;
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

        protected override sealed async Task<Transition> OnExecuteAsync(CancellationToken cancellationToken)
        {
            if (this.transitions.Count == 0)
            {
                return await base.OnExecuteAsync(cancellationToken);
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.cancellationTokenSource.Token);

            var currentCancellationToken = combinedCancellationTokenSource.Token;

            Interlocked.Exchange(ref this.eventChannel, new EventChannel<Transition>(currentCancellationToken));

            try
            {
                currentCancellationToken.Register(() => this.eventChannel?.Acknowledge(false));

                return await base.OnExecuteAsync(currentCancellationToken);
            }
            finally
            {
                var eventChannel = this.eventChannel;
                Interlocked.Exchange(ref this.eventChannel, null);
                eventChannel.Acknowledge(false);

                combinedCancellationTokenSource.Dispose();

                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }
        }

        protected override sealed async Task<Transition> ExecuteStepAsync(CancellationToken cancellationToken)
            => this.transitions.Count == 0
                ? await this.ExecuteEventStepAsync(cancellationToken).ConfigureAwait(false)
                : await this.ExecuteStepWithTransitionsAsync(cancellationToken).ConfigureAwait(false);

        private async Task<Transition> ExecuteStepWithTransitionsAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(this.eventChannel != null);
            Debug.Assert(this.cancellationTokenSource != null);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            try
            {
                var stateTransitionTask = this.ExecuteEventStepAsync(cancellationToken);

                var transitionTask = await Task.WhenAny(this.eventChannel.ReceiveAsync(), stateTransitionTask);

                if (transitionTask != stateTransitionTask)
                {
                    this.cancellationTokenSource.Cancel();
                }
                else
                {
                    var transition = await stateTransitionTask.ConfigureAwait(false);

                    if (transition != null)
                    {
                        this.eventChannel.Acknowledge(false);
                        return transition;
                    }
                }

                {
                    var transition = await this.eventChannel.ReceiveAsync().ConfigureAwait(false);
                    this.eventChannel.Acknowledge(true);
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

        protected abstract Task<Transition> ExecuteEventStepAsync(CancellationToken cancellationToken);
    }
}