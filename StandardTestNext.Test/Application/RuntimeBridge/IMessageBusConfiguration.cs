namespace StandardTestNext.Test.RuntimeBridge;

public interface IMessageBusConfiguration
{
    string? Provider { get; }
    string? Host { get; }
    int? Port { get; }
    string? ClientId { get; }
    string? TopicPrefix { get; }
    string? Username { get; }
    string? Password { get; }
}
