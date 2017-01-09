namespace Morgados.StateMachineExploration.Tests.Definitions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Morgados.StateMachines.Definitions;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class SimpleStateTests
    {
        [Fact]
        public async Task SimpleState_BuildRuntimeStateAndExecute()
        {
            var tracker = new TestTracker();

            var state = new SimpleState("test")
            {
                OnEnterAction = tracker.StateEnterAction,
                OnExecuteAction = tracker.StateExecutionAction,
                OnExitAction = tracker.StateExitAction,
                OnCanceledAction = tracker.StateCanceledAction,
            };

            await state
                .BuildRuntimeState()
                .ExecuteAsync(CancellationToken.None);

            Assert.Equal(">test;*test;<test;", tracker.ToString());
        }
    }
}
