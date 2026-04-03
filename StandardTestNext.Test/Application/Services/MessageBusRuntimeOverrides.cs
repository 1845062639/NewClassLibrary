namespace StandardTestNext.Test.Application.Services;

public static class MessageBusRuntimeOverrides
{
    public static void ApplyEnvironmentOverrides(MessageBusConfiguration messageBus)
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

    public static void ApplyCommandLineOverrides(MessageBusConfiguration messageBus, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--message-bus", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.Provider = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.Provider = arg[("--message-bus=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--message-bus-host", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.Host = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus-host=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.Host = arg[("--message-bus-host=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--message-bus-port", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i].Trim(), out var parsedPort))
                {
                    messageBus.Port = parsedPort;
                }
                continue;
            }

            if (arg.StartsWith("--message-bus-port=", StringComparison.OrdinalIgnoreCase))
            {
                var portValue = arg[("--message-bus-port=".Length)..].Trim();
                if (int.TryParse(portValue, out var parsedPort))
                {
                    messageBus.Port = parsedPort;
                }
                continue;
            }

            if (string.Equals(arg, "--message-bus-client-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.ClientId = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus-client-id=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.ClientId = arg[("--message-bus-client-id=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--message-bus-topic-prefix", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.TopicPrefix = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus-topic-prefix=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.TopicPrefix = arg[("--message-bus-topic-prefix=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--message-bus-username", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.Username = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus-username=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.Username = arg[("--message-bus-username=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--message-bus-password", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                messageBus.Password = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--message-bus-password=", StringComparison.OrdinalIgnoreCase))
            {
                messageBus.Password = arg[("--message-bus-password=".Length)..].Trim();
            }
        }
    }
}
