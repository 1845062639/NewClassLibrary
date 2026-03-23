namespace StandardTestNext.Test.Application.Services;

public sealed class TestStartupOptions
{
    public string PersistenceMode { get; init; } = "memory";
    public string? SQLiteDbPath { get; init; }
    public bool RunSmokeTests { get; init; }
    public MessageBusConfiguration MessageBus { get; init; } = new();
}
