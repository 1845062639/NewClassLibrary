namespace StandardTestNext.Test.RuntimeBridge;

public sealed class MessageBusOptions
{
    public string Provider { get; init; } = "inmemory";
    public string? Host { get; init; }
    public int? Port { get; init; }
    public string? ClientId { get; init; }
    public string? TopicPrefix { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public int PublishTimeoutSeconds { get; init; } = 5;
    public int SubscribeTimeoutSeconds { get; init; } = 5;
}
