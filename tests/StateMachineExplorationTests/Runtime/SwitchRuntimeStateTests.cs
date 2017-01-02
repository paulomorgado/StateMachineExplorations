namespace Morgados.StateMachineExploration.Tests.Runtime
{
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
        public async Task SwitchRuntimeState_WhenSelectorReturnsExistingOption_ReturnsTransitionForOption()
        {
            var tracker = new TestTracker();

            var selectedTransition = new RuntimeTransition("1", A.Fake<ITransitionTarget>(), null, null);
            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new SwitchRuntimeState<int>(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction,
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
        public async Task SwitchRuntimeState_WhenSelectorReturnsNonExistingOption_ReturnsElseTransition()
        {
            var tracker = new TestTracker();

            var elseTransition = new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null);

            var state = new SwitchRuntimeState<int>(
                "test",
                tracker.StateEnterAction,
                tracker.StateExitAction,
                tracker.StateCancelledAction,
                elseTransition,
                new Dictionary<int, RuntimeTransition>(),
                () => 2);

            var actual = await state.ExecuteAsync(CancellationToken.None);

            Assert.Equal(elseTransition, actual);
        }

        [Fact]
        public async Task SwitchRuntimeState_WithSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchRuntimeState<int>(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCancelledAction,
                    new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, RuntimeTransition>
                    {
                        { 0, new RuntimeTransition("0", null, null, null) },
                        { 1, new RuntimeTransition("1", A.Fake<ITransitionTarget>(), null, null)},
                        { 2, new RuntimeTransition("2", null, null, null) },
                    },
                    () => { cts.Cancel(); return 1; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }

        [Fact]
        public async Task SwitchRuntimeState_WithoutSelectedTransitionAndCancelled_ReturnNullAndRunsCancelledAction()
        {
            var tracker = new TestTracker();

            using (var cts = new CancellationTokenSource())
            {
                var state = new SwitchRuntimeState<int>(
                    "test",
                    tracker.StateEnterAction,
                    tracker.StateExitAction,
                    tracker.StateCancelledAction,
                    new RuntimeTransition("False", A.Fake<ITransitionTarget>(), null, null),
                    new Dictionary<int, RuntimeTransition>(),
                    () => { cts.Cancel(); return 2; });

                var actual = await state.ExecuteAsync(cts.Token);

                Assert.Null(actual);
            }

            Assert.Equal(">test;!test;", tracker.ToString());
        }
    }
}
