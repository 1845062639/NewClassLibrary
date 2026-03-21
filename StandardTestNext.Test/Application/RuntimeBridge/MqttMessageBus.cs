using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using StandardTestNext.Contracts;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;

namespace StandardTestNext.Test.Application.RuntimeBridge;

public sealed class MqttMessageBus : IMessageBus, IDisposable
{
    private readonly IMqttClient _client;
    private readonly MessageBusOptions _options;
    private readonly string _topicPrefix;
    private readonly ConcurrentDictionary<string, List<Action<JsonElement>>> _handlersByTopic = new(StringComparer.Ordinal);
    private readonly HashSet<string> _subscribedTopics = new(StringComparer.Ordinal);
    private readonly object _sync = new();
    private CancellationTokenSource? _reconnectCts;
    private Task? _reconnectLoopTask;
    private bool _disposed;
    private bool _resubscribing;
    private bool _reconnectScheduled;

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

        using var timeoutCts = CreateTimeoutCts(_options.PublishTimeoutSeconds);

        try
        {
            _client.PublishAsync(mqttMessage, timeoutCts.Token).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (TryWrapTimeout(ex, timeoutCts, $"Publish to '{resolvedTopic}' timed out after {_options.PublishTimeoutSeconds}s.", out var timeoutException))
        {
            ScheduleReconnect();
            throw timeoutException;
        }
        catch
        {
            ScheduleReconnect();
            throw;
        }

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
        CancellationTokenSource? reconnectCts;
        Task? reconnectLoopTask;

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
            reconnectCts = _reconnectCts;
            reconnectLoopTask = _reconnectLoopTask;
            _reconnectCts = null;
            _reconnectLoopTask = null;
            _reconnectScheduled = false;
            _handlersByTopic.Clear();
            _subscribedTopics.Clear();
        }

        reconnectCts?.Cancel();
        TryWaitReconnectLoop(reconnectLoopTask);
        reconnectCts?.Dispose();

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

        ScheduleReconnect();
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        ThrowIfDisposed();

        if (_client.IsConnected)
        {
            return;
        }

        try
        {
            _client.ConnectAsync(BuildClientOptions(), CancellationToken.None).GetAwaiter().GetResult();
        }
        catch
        {
            ScheduleReconnect();
            throw;
        }
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
        lock (_sync)
        {
            if (_subscribedTopics.Contains(topic))
            {
                return;
            }
        }

        using var timeoutCts = CreateTimeoutCts(_options.SubscribeTimeoutSeconds);

        try
        {
            _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build(), timeoutCts.Token).GetAwaiter().GetResult();
        }
        catch (Exception ex) when (TryWrapTimeout(ex, timeoutCts, $"Subscribe to '{topic}' timed out after {_options.SubscribeTimeoutSeconds}s.", out var timeoutException))
        {
            ScheduleReconnect();
            throw timeoutException;
        }
        catch
        {
            ScheduleReconnect();
            throw;
        }

        lock (_sync)
        {
            _subscribedTopics.Add(topic);
        }
    }

    private Action<JsonElement> CreateTypedHandler<T>(Action<T> handler)
    {
        return payload =>
        {
            var message = payload.Deserialize<T>();
            if (message is not null)
            {
                handler(message);
            }
        };
    }

    private string BuildTopic(string topic)
    {
        var normalized = topic?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Message topic cannot be empty.", nameof(topic));
        }

        return string.IsNullOrWhiteSpace(_topicPrefix)
            ? normalized
            : $"{_topicPrefix}/{normalized}";
    }

    private static string NormalizeTopicPrefix(string? prefix)
    {
        return string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : prefix.Trim().Trim('/');
    }

    private static CancellationTokenSource CreateTimeoutCts(int seconds)
    {
        var effectiveSeconds = seconds <= 0 ? 5 : seconds;
        return new CancellationTokenSource(TimeSpan.FromSeconds(effectiveSeconds));
    }

    private static bool TryWrapTimeout(Exception exception, CancellationTokenSource timeoutCts, string message, out TimeoutException timeoutException)
    {
        if (!timeoutCts.IsCancellationRequested)
        {
            timeoutException = null!;
            return false;
        }

        timeoutException = new TimeoutException(message, exception);
        return true;
    }

    private void ScheduleReconnect()
    {
        lock (_sync)
        {
            if (_disposed || _reconnectScheduled)
            {
                return;
            }

            _reconnectScheduled = true;
            _reconnectCts = new CancellationTokenSource();
            _reconnectLoopTask = Task.Run(() => ReconnectLoopAsync(_reconnectCts.Token));
        }
    }

    private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_client.IsConnected)
                    {
                        await _client.ConnectAsync(BuildClientOptions(), cancellationToken);
                    }

                    lock (_sync)
                    {
                        _reconnectScheduled = false;
                    }

                    return;
                }
                catch when (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_sync)
            {
                _reconnectScheduled = false;
            }
        }
    }

    private static void TryWaitReconnectLoop(Task? reconnectLoopTask)
    {
        try
        {
            reconnectLoopTask?.GetAwaiter().GetResult();
        }
        catch
        {
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
