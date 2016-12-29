namespace Morgados.StateMachine.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EventChannel<TMessage>
    {
        private readonly CancellationToken cancellationToken;
        private readonly object sync = new object();
        private TaskCompletionSource<TMessage> publish;
        private TaskCompletionSource<bool> acknowledge;

        public EventChannel(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        private void EnusreCreated()
        {
            lock (this.sync)
            {
                if (this.publish == null)
                {
                    this.publish = new TaskCompletionSource<TMessage>(TaskCreationOptions.None);
                    this.acknowledge = new TaskCompletionSource<bool>(TaskCreationOptions.None);

                    this.cancellationToken.Register(OnCancellationTokenCancelled);
                }
            }
        }

        public Task<TMessage> ReceiveAsync()
        {
            this.EnusreCreated();

            return this.publish.Task;
        }

        public async Task<bool> SendAsync(TMessage message)
        {
            this.EnusreCreated();

            var acknowledge = this.acknowledge;

            this.publish.TrySetResult(message);

            return await acknowledge.Task;
        }

        public void Acknowledge(bool handled)
        {
            try
            {
                this.acknowledge?.TrySetResult(handled);
            }
            finally
            {
                this.publish = null;
                this.acknowledge = null;
            }
        }

        private void OnCancellationTokenCancelled()
        {
            this.publish?.TrySetCanceled(this.cancellationToken);
            this.acknowledge?.TrySetResult(false);
        }
    }
}
