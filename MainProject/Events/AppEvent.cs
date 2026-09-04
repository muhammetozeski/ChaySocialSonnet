namespace ChaySocialSonnet.MainProject.Events
{
    /// <summary>
    /// Shared "run every action, collect failures, then throw once" used by both
    /// <see cref="AppEvent.Raise"/> and <see cref="AppEvent{T}.Raise"/>, so one broken handler can never
    /// stop the rest of the chain from running.
    /// </summary>
    static class AppEventRaiser
    {
        public static void RunAll(IReadOnlyList<Action> actions)
        {
            List<Exception>? exceptions = null;

            foreach (Action action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    (exceptions ??= []).Add(exception);
                }
            }

            if (exceptions is not null)
            {
                throw new AggregateException("One or more AppEvent handlers threw.", exceptions);
            }
        }
    }

    /// <summary>
    /// Multi-subscriber notification slot with no payload. Backed by a <see cref="HashSet{T}"/> so the
    /// same handler delegate can never end up registered twice, and only exposes <see cref="Subscribe"/>/
    /// <see cref="Unsubscribe"/> so callers cannot reach or replace the backing set directly.
    /// </summary>
    public sealed class AppEvent
    {
        readonly HashSet<Action> handlers = [];

        /// <summary> Registers a handler to run on every <see cref="Raise"/>. Adding the same handler twice is a no-op. </summary>
        public void Subscribe(Action handler) => handlers.Add(handler);

        /// <summary> Removes a previously registered handler. Removing a handler that was never added is a no-op. </summary>
        public void Unsubscribe(Action handler) => handlers.Remove(handler);

        /// <summary>
        /// Invokes every subscribed handler against a snapshot of the current subscriber set, so a
        /// handler that subscribes/unsubscribes during the raise cannot corrupt this pass. A handler
        /// that throws does not stop the rest from running; if any threw, their exceptions are
        /// re-thrown together as a single <see cref="AggregateException"/> once every handler has run.
        /// </summary>
        public void Raise() => AppEventRaiser.RunAll(handlers.ToArray());
    }

    /// <summary>
    /// Multi-subscriber notification slot that passes a <typeparamref name="T"/> payload to every handler.
    /// Same <see cref="HashSet{T}"/>-backed duplicate-subscription protection as <see cref="AppEvent"/>.
    /// </summary>
    public sealed class AppEvent<T>
    {
        readonly HashSet<Action<T>> handlers = [];

        /// <summary> Registers a handler to run on every <see cref="Raise"/>. Adding the same handler twice is a no-op. </summary>
        public void Subscribe(Action<T> handler) => handlers.Add(handler);

        /// <summary> Removes a previously registered handler. Removing a handler that was never added is a no-op. </summary>
        public void Unsubscribe(Action<T> handler) => handlers.Remove(handler);

        /// <summary>
        /// Invokes every subscribed handler with <paramref name="payload"/> against a snapshot of the
        /// current subscriber set, so a handler that subscribes/unsubscribes during the raise cannot
        /// corrupt this pass. A handler that throws does not stop the rest from running; if any threw,
        /// their exceptions are re-thrown together as a single <see cref="AggregateException"/> once
        /// every handler has run.
        /// </summary>
        public void Raise(T payload) =>
            AppEventRaiser.RunAll(handlers.ToArray().Select(handler => (Action)(() => handler(payload))).ToArray());
    }
}
