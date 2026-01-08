namespace Ara3D.Events
{
    /// <summary>
    /// Event types should derive from this class. An event is a data packet
    /// broadcast for anyone listening.  
    /// </summary>
    public interface IEvent
    { }

    /// <summary>
    /// Optional async subscriber interface. (No default interface impls required.)
    /// </summary>
    public interface IAsyncSubscriber<in T> : ISubscriber where T : IEvent
    {
        ValueTask OnEventAsync(T evt, CancellationToken ct);
    }

    /// <summary>
    /// Optional async event bus interface.
    /// </summary>
    public interface IAsyncEventBus : IEventBus
    {
        ValueTask PublishAsync<T>(T evt, CancellationToken ct = default, PublishMode mode = PublishMode.Sequential)
            where T : IEvent;
    }

    public enum PublishMode
    {
        Sequential,
        Parallel
    }

    /// <summary>
    /// Used to subscribe to, and publish events between services.
    /// This decouples event publishers from subscribers.
    /// Subscribers are removed automatically when no-longer used,
    /// because internally it uses WeakReference
    /// </summary>
    public interface IEventBus
    {
        void Publish<T>(T evt) where T : IEvent;
        void Subscribe<T>(ISubscriber<T> subscriber) where T : IEvent;
        void Unsubscribe<T>(ISubscriber<T> subscriber) where T : IEvent;
    }
}
