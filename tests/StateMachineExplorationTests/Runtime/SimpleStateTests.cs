namespace Morgados.StateMachineExploration.Tests.Runtime
{
    public class SimpleStateTests
    {
        //[Fact]
        //public async Task State_WithoutTransitions_CompletesImmediatelly()
        //{
        //    var logger = new TestLogger();

        //    var state = new SimpleState(
        //        "test", 
        //        logger.StateEnterAction, 
        //        logger.StateExecutionAction,
        //        logger.StateExitAction, 
        //        logger.StateCancelledAction);

        //    await state.ExecuteAsync(CancellationToken.None);

        //    Assert.Equal(">test;*test;<test;", logger.ToString());
        //}

        //[Fact]
        //public async Task State_WithCancellationWithoutTransitions_CompletesImmediatellyAndRunsCancelledActionAndThrows()
        //{
        //    var cts = new CancellationTokenSource();
        //    var tcs = new TaskCompletionSource<object>();

        //    var logger = new TestLogger();

        //    var state = new SimpleState(
        //        "test",
        //        logger.StateEnterAction, 
        //        async s => { await logger.StateExecutionAction(s); await tcs.Task; }, 
        //        logger.StateExitAction, 
        //        logger.StateCancelledAction);

        //    Task<Transition> task = state.ExecuteAsync(cts.Token);

        //    cts.Cancel();
        //    tcs.SetResult(null);

        //    Assert.ThrowsAsync<OperationCanceledException>(async () => await task);

        //    Assert.Equal(">test;*test;!test;<test;", logger.ToString());
        //}
    }
}
