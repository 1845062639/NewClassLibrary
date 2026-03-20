namespace StandardTestNext.Test.Application.Services;

public sealed class TestRuntimeConfiguration
{
    public string PersistenceMode { get; init; } = "memory";
    public string? SQLiteDbPath { get; init; }

    public TestStartupOptions ToStartupOptions()
    {
        return new TestStartupOptions
        {
            PersistenceMode = PersistenceMode,
            SQLiteDbPath = SQLiteDbPath
        };
    }
}
