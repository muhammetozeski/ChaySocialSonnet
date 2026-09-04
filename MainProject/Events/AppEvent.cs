namespace ChaySocialSonnet.MainProject.Events
{
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

        /// <summary> Invokes every subscribed handler against a snapshot of the current subscriber set, so a handler that subscribes/unsubscribes during the raise cannot corrupt this pass. </summary>
        public void Raise()
        {
            foreach (Action handler in handlers.ToArray())
            {
                handler();
            }
        }
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

        /// <summary> Invokes every subscribed handler with <paramref name="payload"/> against a snapshot of the current subscriber set, so a handler that subscribes/unsubscribes during the raise cannot corrupt this pass. </summary>
        public void Raise(T payload)
        {
            foreach (Action<T> handler in handlers.ToArray())
            {
                handler(payload);
            }
        }
    }
}
