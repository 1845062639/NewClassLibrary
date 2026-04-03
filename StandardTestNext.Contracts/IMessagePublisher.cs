namespace StandardTestNext.Contracts;

public interface IMessagePublisher
{
    void Publish<T>(string topic, T message);
}
