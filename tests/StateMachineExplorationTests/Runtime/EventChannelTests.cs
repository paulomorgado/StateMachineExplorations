namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Morgados.StateMachine.Runtime;
    using Xunit;

    public class EventChannelTests
    {
        [Fact]
        public async Task EventChannel_SentMessage_IsReceivedByReiceiver()
        {
            var eventChannel = new EventChannel<object>(CancellationToken.None);
            var expected = new object();

            eventChannel.SendAsync(expected);

            Assert.Equal(expected, await eventChannel.ReceiveAsync());

            eventChannel.Acknowledge(false);
        }

        [Fact]
        public async Task EventChannel_Acknowledge_IsReceivedBySender()
        {
            var eventChannel = new EventChannel<object>(CancellationToken.None);
            var expected = true;

            var task = eventChannel.SendAsync(expected);
            await eventChannel.ReceiveAsync();
            eventChannel.Acknowledge(expected);

            Assert.Equal(expected, await task);
        }

        [Fact]
        public async Task EventChannel_Cancelled_SendYieldsFalse()
        {
            using (var tcs = new CancellationTokenSource())
            {
                var eventChannel = new EventChannel<object>(tcs.Token);
                var expected = false;

                var task = eventChannel.SendAsync(expected);

                tcs.Cancel();

                Assert.Equal(expected, await task);
            }
        }

        [Fact]
        public async Task EventChannel_Cancelled_ReceiveThrows()
        {
            using (var tcs = new CancellationTokenSource())
            {
                var eventChannel = new EventChannel<object>(tcs.Token);

                var task = eventChannel.ReceiveAsync();

                tcs.Cancel();

                Assert.ThrowsAsync<OperationCanceledException>(async () => await task);

                Assert.True(task.IsCanceled);
            }
        }
    }
}
