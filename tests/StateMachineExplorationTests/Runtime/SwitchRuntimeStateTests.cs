namespace Morgados.StateMachineExploration.Tests.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Moq;
    using Morgados.StateMachines.Runtime;
    using Xunit;

    public class SwitchRuntimeStateTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenSelectorReturnsExistingOption_ReturnsTransitionForOption()
        {
            var tracker = new TestTracker();

            var selectedTransition = new RuntimeTransition("1", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new SwitchRuntimeState<int>(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                elseTransition,
                new Dictionary<int, RuntimeTransition>
                {
                    { 0, new RuntimeTransition("0", null, null, null) },
                    { 1, selectedTransition },
                    { 2, new RuntimeTransition("2", null, null, null) },
                },
                () => 1);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(selectedTransition, actual);
        }

        [Fact]
        public async Task ExecuteAsync_WhenSelectorReturnsNonExistingOption_ReturnsElseTransition()
        {
            var tracker = new TestTracker();

            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new SwitchRuntimeState<int>(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCanceledAction,
                elseTransition,
                new Dictionary<int, RuntimeTransition>(),
                () => 2);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);
        }

        [Fact]
        public async Task ExecuteAsync_WithSelectedTransitionAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchRuntimeState<int>(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCanceledAction,
                    new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, RuntimeTransition>
                    {
                        { 0, new RuntimeTransition("0", null, null, null) },
                        { 1, new RuntimeTransition("1", A.Fake<ITransitionTarget>(), null, null)},
                        { 2, new RuntimeTransition("2", null, null, null) },
                    },
                    () =>
                    {
                        cts.Cancel();
                        return 1;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task ExecuteAsync_WithoutSelectedTransitionAndCanceled_RunsCanceledActionAndThrowsOperationCanceledException()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchRuntimeState<int>(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCanceledAction,
                    new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, RuntimeTransition>(),
                    () =>
                    {
                        cts.Cancel();
                        return 2;
                    });

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await state.ExecuteAsync(cts.Token));
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
