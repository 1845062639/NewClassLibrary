namespace StandardTestNext.Contracts;

[Obsolete("Use IMessageBus directly; this interface remains only as a temporary compatibility shim.")]
public interface IMessageSubscriber
{
    void Subscribe<T>(string topic, Action<T> handler);
}
