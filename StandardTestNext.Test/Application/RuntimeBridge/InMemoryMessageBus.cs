using System.Text.Json;

namespace StandardTestNext.Test.RuntimeBridge;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<string, List<Delegate>> _handlers = new();

    public void Publish<T>(string topic, T message)
    {
        Console.WriteLine($"[Bus] {topic}");
        Console.WriteLine(JsonSerializer.Serialize(message));

        if (!_handlers.TryGetValue(topic, out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.OfType<Action<T>>())
        {
            handler(message);
        }
    }

    public void Subscribe<T>(string topic, Action<T> handler)
    {
        if (!_handlers.TryGetValue(topic, out var handlers))
        {
            handlers = new List<Delegate>();
            _handlers[topic] = handlers;
        }

        handlers.Add(handler);
    }
}
