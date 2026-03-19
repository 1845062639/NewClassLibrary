using System.Text.Json;

namespace StandardTestNext.App.ContractsBridge;

public sealed class InMemoryMessageBus : IMessagePublisher
{
    public void Publish<T>(string topic, T message)
    {
        Console.WriteLine($"[App->Bus] {topic}");
        Console.WriteLine(JsonSerializer.Serialize(message));
    }
}
