namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class EventChannelTests
    {
        [Fact]
        public async Task EventChannel_SentMessage_IsReceivedByReiceiver()
        {
            var eventChannel = new EventChannel<object>();
            var expected = new object();

            var task = eventChannel.SendAsync(expected);

            Assert.Equal(expected, await eventChannel.ReceiveAsync());
        }

        [Fact]
        public async Task EventChannel_Acknowledge_IsReceivedBySender()
        {
            var eventChannel = new EventChannel<object>();
            var expected = true;

            var task = eventChannel.SendAsync(expected);
            eventChannel.Acknowledge(expected);

            Assert.Equal(expected, await task);
        }
    }
}
