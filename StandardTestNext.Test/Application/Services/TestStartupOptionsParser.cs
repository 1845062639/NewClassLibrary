namespace StandardTestNext.Test.Application.Services;

public static class TestStartupOptionsParser
{
    public static TestStartupOptions Parse(string[] args)
    {
        string? configPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                configPath = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
            {
                configPath = arg[("--config=".Length)..].Trim();
            }
        }

        var fileConfiguration = TestRuntimeConfigurationLoader.Load(configPath);
        var persistenceMode = fileConfiguration.PersistenceMode?.Trim();
        var sqliteDbPath = fileConfiguration.SQLiteDbPath?.Trim();

        var envPersistenceMode = Environment.GetEnvironmentVariable("STNEXT_TEST_PERSISTENCE")?.Trim();
        if (!string.IsNullOrWhiteSpace(envPersistenceMode))
        {
            persistenceMode = envPersistenceMode;
        }

        var envSqliteDbPath = Environment.GetEnvironmentVariable("STNEXT_TEST_SQLITE_DB")?.Trim();
        if (!string.IsNullOrWhiteSpace(envSqliteDbPath))
        {
            sqliteDbPath = envSqliteDbPath;
        }

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--persistence", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                persistenceMode = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--persistence=", StringComparison.OrdinalIgnoreCase))
            {
                persistenceMode = arg[("--persistence=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--sqlite-db", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                sqliteDbPath = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--sqlite-db=", StringComparison.OrdinalIgnoreCase))
            {
                sqliteDbPath = arg[("--sqlite-db=".Length)..].Trim();
            }
        }

        return new TestStartupOptions
        {
            PersistenceMode = string.IsNullOrWhiteSpace(persistenceMode) ? "memory" : persistenceMode,
            SQLiteDbPath = string.IsNullOrWhiteSpace(sqliteDbPath) ? null : sqliteDbPath
        };
    }
}
