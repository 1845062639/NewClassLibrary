namespace StandardTestNext.Test.Application.Services;

public sealed class TestRuntimeConfiguration
{
    public string PersistenceMode { get; init; } = "memory";
    public string? SQLiteDbPath { get; init; }
    public MessageBusConfiguration MessageBus { get; init; } = new();

    public string MessageBusProvider => MessageBus.Provider;

    public TestStartupOptions ToStartupOptions()
    {
        return new TestStartupOptions
        {
            PersistenceMode = PersistenceMode,
            SQLiteDbPath = SQLiteDbPath
        };
    }
}

public sealed class MessageBusConfiguration
{
    public string Provider { get; init; } = "inmemory";
    public string? Host { get; init; }
    public int? Port { get; init; }
    public string? ClientId { get; init; }
    public string? TopicPrefix { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
}
