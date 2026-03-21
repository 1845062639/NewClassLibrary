namespace StandardTestNext.Test.RuntimeBridge;

public interface IMessageBus : IMessagePublisher
{
    void Subscribe<T>(string topic, Action<T> handler);
}
