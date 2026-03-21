namespace StandardTestNext.Test.RuntimeBridge;

public interface IMessagePublisher
{
    void Publish<T>(string topic, T message);
}
