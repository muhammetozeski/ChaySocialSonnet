using ChaySocialSonnet.MainProject.Events;

namespace ChaySocialSonnet.MainProject.Tests.Events
{
    public class AppEventTests
    {
        [Fact]
        public void Raise_InvokesSubscribedHandler()
        {
            var appEvent = new AppEvent();
            var callCount = 0;
            appEvent.Subscribe(() => callCount++);

            appEvent.Raise();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Subscribe_SameHandlerTwice_OnlyInvokedOnce()
        {
            var appEvent = new AppEvent();
            var callCount = 0;
            void Handler() => callCount++;

            appEvent.Subscribe(Handler);
            appEvent.Subscribe(Handler);
            appEvent.Raise();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Unsubscribe_StopsFurtherInvocations()
        {
            var appEvent = new AppEvent();
            var callCount = 0;
            void Handler() => callCount++;

            appEvent.Subscribe(Handler);
            appEvent.Unsubscribe(Handler);
            appEvent.Raise();

            Assert.Equal(0, callCount);
        }

        [Fact]
        public void Raise_HandlerUnsubscribingItself_DoesNotThrowAndStopsNextRaise()
        {
            var appEvent = new AppEvent();
            var secondHandlerCallCount = 0;
            void SelfRemoving() => appEvent.Unsubscribe(SelfRemoving);
            void SecondHandler() => secondHandlerCallCount++;
            appEvent.Subscribe(SelfRemoving);
            appEvent.Subscribe(SecondHandler);

            var exception = Record.Exception(() => appEvent.Raise());
            appEvent.Raise();

            Assert.Null(exception);
            Assert.Equal(2, secondHandlerCallCount);
        }

        [Fact]
        public void GenericRaise_PassesPayloadToHandler()
        {
            var appEvent = new AppEvent<int>();
            var receivedPayload = 0;
            appEvent.Subscribe(payload => receivedPayload = payload);

            appEvent.Raise(42);

            Assert.Equal(42, receivedPayload);
        }

        [Fact]
        public void Raise_OneHandlerThrows_OtherHandlersStillRunAndExceptionIsThrownAfter()
        {
            var appEvent = new AppEvent();
            var laterHandlerCallCount = 0;
            appEvent.Subscribe(() => throw new InvalidOperationException("boom"));
            appEvent.Subscribe(() => laterHandlerCallCount++);

            var exception = Record.Exception(() => appEvent.Raise());

            Assert.Equal(1, laterHandlerCallCount);
            var aggregateException = Assert.IsType<AggregateException>(exception);
            Assert.Single(aggregateException.InnerExceptions);
            Assert.IsType<InvalidOperationException>(aggregateException.InnerExceptions[0]);
        }

        [Fact]
        public void GenericRaise_OneHandlerThrows_OtherHandlersStillRunAndExceptionIsThrownAfter()
        {
            var appEvent = new AppEvent<int>();
            var receivedPayload = 0;
            appEvent.Subscribe(_ => throw new InvalidOperationException("boom"));
            appEvent.Subscribe(payload => receivedPayload = payload);

            var exception = Record.Exception(() => appEvent.Raise(42));

            Assert.Equal(42, receivedPayload);
            Assert.IsType<AggregateException>(exception);
        }
    }
}
