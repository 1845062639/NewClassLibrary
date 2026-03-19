namespace StandardTestNext.App.ContractsBridge;

public interface IMessagePublisher
{
    void Publish<T>(string topic, T message);
}
