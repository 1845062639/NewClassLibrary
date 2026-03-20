using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Text.Json;

namespace StandardTestNext.App.ContractsBridge;

public sealed class MqttMessageBus : IMessageBus, IDisposable
{
    private readonly IMqttClient _client;
    private readonly MessageBusOptions _options;
    private readonly string _topicPrefix;
    private readonly ConcurrentDictionary<string, List<Action<JsonElement>>> _handlersByTopic = new(StringComparer.Ordinal);
    private readonly HashSet<string> _subscribedTopics = new(StringComparer.Ordinal);
    private readonly object _sync = new();
    private bool _disposed;
    private bool _resubscribing;

    public MqttMessageBus(MessageBusOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _topicPrefix = NormalizeTopicPrefix(options.TopicPrefix);

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.DisconnectedAsync += OnDisconnectedAsync;
        _client.ConnectedAsync += OnConnectedAsync;
    }

    public void Publish<T>(string topic, T message)
    {
        ThrowIfDisposed();
        EnsureConnected();

        var resolvedTopic = BuildTopic(topic);
        var payload = JsonSerializer.Serialize(message);
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(resolvedTopic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        _client.PublishAsync(mqttMessage, CancellationToken.None).GetAwaiter().GetResult();
        Console.WriteLine($"[Bus:MQTT] {resolvedTopic}");
        Console.WriteLine(payload);
    }

    public void Subscribe<T>(string topic, Action<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        ThrowIfDisposed();

        var resolvedTopic = BuildTopic(topic);
        var typedHandler = CreateTypedHandler(handler);

        lock (_sync)
        {
            if (!_handlersByTopic.TryGetValue(resolvedTopic, out var handlers))
            {
                handlers = new List<Action<JsonElement>>();
                _handlersByTopic[resolvedTopic] = handlers;
            }

            handlers.Add(typedHandler);
        }

        EnsureConnected();
        EnsureSubscribed(resolvedTopic);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _handlersByTopic.Clear();
            _subscribedTopics.Clear();
        }

        if (_client.IsConnected)
        {
            _client.DisconnectAsync().GetAwaiter().GetResult();
        }

        _client.Dispose();
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        List<Action<JsonElement>> handlers;
        lock (_sync)
        {
            if (!_handlersByTopic.TryGetValue(eventArgs.ApplicationMessage.Topic, out var registeredHandlers)
                || registeredHandlers.Count == 0)
            {
                return Task.CompletedTask;
            }

            handlers = registeredHandlers.ToList();
        }

        var payload = eventArgs.ApplicationMessage.PayloadSegment;
        if (payload.Array is null || payload.Count == 0)
        {
            return Task.CompletedTask;
        }

        using var document = JsonDocument.Parse(payload);
        foreach (var handler in handlers)
        {
            handler(document.RootElement.Clone());
        }

        return Task.CompletedTask;
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs _)
    {
        ResubscribeAll();
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs _)
    {
        if (_disposed)
        {
            return Task.CompletedTask;
        }

        lock (_sync)
        {
            _subscribedTopics.Clear();
        }

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        ThrowIfDisposed();

        if (_client.IsConnected)
        {
            return;
        }

        _client.ConnectAsync(BuildClientOptions(), CancellationToken.None).GetAwaiter().GetResult();
    }

    private MqttClientOptions BuildClientOptions()
    {
        var host = _options.Host?.Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("MQTT message bus requires a non-empty host.");
        }

        var clientId = _options.ClientId?.Trim();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("MQTT message bus requires a non-empty clientId.");
        }

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, _options.Port ?? 1883)
            .WithClientId(clientId)
            .WithCleanSession(false);

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            builder.WithCredentials(_options.Username, _options.Password);
        }

        return builder.Build();
    }

    private void ResubscribeAll()
    {
        lock (_sync)
        {
            if (_resubscribing)
            {
                return;
            }

            _resubscribing = true;
        }

        try
        {
            List<string> topics;
            lock (_sync)
            {
                topics = _handlersByTopic.Keys.ToList();
                _subscribedTopics.Clear();
            }

            foreach (var topic in topics)
            {
                EnsureSubscribed(topic);
            }
        }
        finally
        {
            lock (_sync)
            {
                _resubscribing = false;
            }
        }
    }

    private void EnsureSubscribed(string topic)
    {
        var shouldSubscribe = false;

        lock (_sync)
        {
            if (_subscribedTopics.Add(topic))
            {
                shouldSubscribe = true;
            }
        }

        if (!shouldSubscribe)
        {
            return;
        }

        try
        {
            SubscribeCoreAsync(topic, CancellationToken.None).GetAwaiter().GetResult();
        }
        catch
        {
            lock (_sync)
            {
                _subscribedTopics.Remove(topic);
            }

            throw;
        }
    }

    private static Action<JsonElement> CreateTypedHandler<T>(Action<T> handler)
    {
        return payload =>
        {
            var message = payload.Deserialize<T>();
            if (message is null)
            {
                throw new JsonException($"Failed to deserialize MQTT payload to {typeof(T).FullName}.");
            }

            handler(message);
        };
    }

    private Task SubscribeCoreAsync(string topic, CancellationToken cancellationToken)
    {
        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
            .Build();

        return _client.SubscribeAsync(options, cancellationToken);
    }

    private string BuildTopic(string topic)
    {
        var trimmed = topic.Trim().Trim('/');
        return string.IsNullOrWhiteSpace(_topicPrefix)
            ? trimmed
            : $"{_topicPrefix}/{trimmed}";
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MqttMessageBus));
        }
    }

    private static string NormalizeTopicPrefix(string? prefix)
    {
        return string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : prefix.Trim().Trim('/');
    }
}
