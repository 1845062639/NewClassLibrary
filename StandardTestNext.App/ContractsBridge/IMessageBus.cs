namespace StandardTestNext.App.ContractsBridge;

public interface IMessageBus : IMessagePublisher
{
    void Subscribe<T>(string topic, Action<T> handler);
}
