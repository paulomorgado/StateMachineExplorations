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
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected EventStateBase(
            string name,
            Func<string, Task> onEnterAction,
            Func<string, Task> onExitAction,
            Func<string, Task> onCancelledAction)
            : base(name, onEnterAction, onExitAction, onCancelledAction)
        {
        }

        public async Task<bool? > PublishEventAsync(string eventName)
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

        protected override sealed async Task<Transition> ExecuteStepAsync(CancellationToken cancellationToken)
        {
            if (this.transitions.Count == 0)
            {
                return await this.ExecuteEventStepAsync(cancellationToken).ConfigureAwait(false);
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.cancellationTokenSource.Token);

            var currentCancellationToken = combinedCancellationTokenSource.Token;

            Interlocked.Exchange(ref this.eventChannel, new EventChannel<Transition>());

            try
            {
                currentCancellationToken.Register(() =>
                    {
                        var eventChannel = this.eventChannel;
                        Interlocked.Exchange(ref this.eventChannel, null);
                        eventChannel?.Acknowledge(null);
                    });

                return await this.ExecuteStepWithTransitionsAsync(currentCancellationToken).ConfigureAwait(false);
            }
            finally
            {
                var eventChannel = this.eventChannel;
                Interlocked.Exchange(ref this.eventChannel, null);
                eventChannel?.Acknowledge(null);

                combinedCancellationTokenSource.Dispose();

                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }
        }

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