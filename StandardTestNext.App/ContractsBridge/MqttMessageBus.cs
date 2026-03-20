using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text.Json;

namespace StandardTestNext.App.ContractsBridge;

public sealed class MqttMessageBus : IMessageBus, IDisposable
{
    private readonly IMqttClient _client;
    private readonly MessageBusOptions _options;
    private readonly string _topicPrefix;
    private readonly List<Func<CancellationToken, Task>> _subscriptionRestorers = new();
    private readonly object _sync = new();

    public MqttMessageBus(MessageBusOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _topicPrefix = NormalizeTopicPrefix(options.TopicPrefix);

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    public void Publish<T>(string topic, T message)
    {
        EnsureConnected();

        var payload = JsonSerializer.Serialize(message);
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(BuildTopic(topic))
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        _client.PublishAsync(mqttMessage, CancellationToken.None).GetAwaiter().GetResult();
        Console.WriteLine($"[Bus:MQTT] {BuildTopic(topic)}");
        Console.WriteLine(payload);
    }

    public void Subscribe<T>(string topic, Action<T> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var resolvedTopic = BuildTopic(topic);
        _client.ApplicationMessageReceivedAsync += e =>
        {
            if (!string.Equals(e.ApplicationMessage.Topic, resolvedTopic, StringComparison.Ordinal))
            {
                return Task.CompletedTask;
            }

            var payload = e.ApplicationMessage.PayloadSegment;
            if (payload.Array is null || payload.Count == 0)
            {
                return Task.CompletedTask;
            }

            var message = JsonSerializer.Deserialize<T>(payload)!;
            handler(message);
            return Task.CompletedTask;
        };

        lock (_sync)
        {
            _subscriptionRestorers.Add(ct => SubscribeCoreAsync(resolvedTopic, ct));
        }

        EnsureConnected();
        SubscribeCoreAsync(resolvedTopic, CancellationToken.None).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        if (_client.IsConnected)
        {
            _client.DisconnectAsync().GetAwaiter().GetResult();
        }

        _client.Dispose();
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs _)
    {
        await Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_client.IsConnected)
        {
            return;
        }

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
            .WithCleanSession();

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            builder.WithCredentials(_options.Username, _options.Password);
        }

        _client.ConnectAsync(builder.Build(), CancellationToken.None).GetAwaiter().GetResult();

        List<Func<CancellationToken, Task>> restorers;
        lock (_sync)
        {
            restorers = _subscriptionRestorers.ToList();
        }

        foreach (var restore in restorers)
        {
            restore(CancellationToken.None).GetAwaiter().GetResult();
        }
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

    private static string NormalizeTopicPrefix(string? prefix)
    {
        return string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : prefix.Trim().Trim('/');
    }
}
