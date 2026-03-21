using StandardTestNext.Contracts;
using System.Text.Json;

namespace StandardTestNext.Test.Application.RuntimeBridge;

public sealed class InMemoryMessageBus : IMessageBus
{
    private readonly Dictionary<string, List<Delegate>> _subscriptions = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public void Publish<T>(string topic, T message)
    {
        List<Delegate> handlers;
        lock (_gate)
        {
            if (!_subscriptions.TryGetValue(topic, out var registeredHandlers) || registeredHandlers.Count == 0)
            {
                return;
            }

            handlers = registeredHandlers.ToList();
        }

        var payload = JsonSerializer.Serialize(message);
        Console.WriteLine($"[Bus:InMemory] {topic}");
        Console.WriteLine(payload);

        foreach (var handler in handlers.OfType<Action<T>>())
        {
            handler(message!);
        }
    }

    public void Subscribe<T>(string topic, Action<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        lock (_gate)
        {
            if (!_subscriptions.TryGetValue(topic, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscriptions[topic] = handlers;
            }

            handlers.Add(handler);
        }
    }
}
