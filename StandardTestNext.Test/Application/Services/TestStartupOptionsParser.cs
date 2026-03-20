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
        var messageBus = CloneMessageBusConfiguration(fileConfiguration.MessageBus);

        ApplyMessageBusEnvironmentOverrides(messageBus);

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
            SQLiteDbPath = string.IsNullOrWhiteSpace(sqliteDbPath) ? null : sqliteDbPath,
            MessageBus = messageBus
        };
    }

    private static MessageBusConfiguration CloneMessageBusConfiguration(MessageBusConfiguration configuration)
    {
        return new MessageBusConfiguration
        {
            Provider = configuration.Provider,
            Host = configuration.Host,
            Port = configuration.Port,
            ClientId = configuration.ClientId,
            TopicPrefix = configuration.TopicPrefix,
            Username = configuration.Username,
            Password = configuration.Password
        };
    }

    private static void ApplyMessageBusEnvironmentOverrides(MessageBusConfiguration messageBus)
    {
        var envMessageBusProvider = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusProvider))
        {
            messageBus.Provider = envMessageBusProvider;
        }

        var envMessageBusHost = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_HOST")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusHost))
        {
            messageBus.Host = envMessageBusHost;
        }

        var envMessageBusPort = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_PORT")?.Trim();
        if (int.TryParse(envMessageBusPort, out var parsedMessageBusPort))
        {
            messageBus.Port = parsedMessageBusPort;
        }

        var envMessageBusClientId = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_CLIENT_ID")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusClientId))
        {
            messageBus.ClientId = envMessageBusClientId;
        }

        var envMessageBusTopicPrefix = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_TOPIC_PREFIX")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusTopicPrefix))
        {
            messageBus.TopicPrefix = envMessageBusTopicPrefix;
        }

        var envMessageBusUsername = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_USERNAME")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusUsername))
        {
            messageBus.Username = envMessageBusUsername;
        }

        var envMessageBusPassword = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS_PASSWORD")?.Trim();
        if (!string.IsNullOrWhiteSpace(envMessageBusPassword))
        {
            messageBus.Password = envMessageBusPassword;
        }
    }
}
