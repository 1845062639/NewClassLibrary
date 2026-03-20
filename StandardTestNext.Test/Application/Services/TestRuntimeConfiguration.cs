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
            SQLiteDbPath = SQLiteDbPath,
            MessageBus = MessageBus
        };
    }
}

public sealed class MessageBusConfiguration
{
    public string Provider { get; set; } = "inmemory";
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? ClientId { get; set; }
    public string? TopicPrefix { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
