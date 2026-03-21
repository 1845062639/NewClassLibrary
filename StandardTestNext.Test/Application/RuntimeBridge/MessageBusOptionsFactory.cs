using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.RuntimeBridge;

public static class MessageBusOptionsFactory
{
    public static MessageBusOptions Create(IMessageBusConfiguration configuration)
    {
        return Create(
            configuration.Provider,
            configuration.Host,
            configuration.Port,
            configuration.ClientId,
            configuration.TopicPrefix,
            configuration.Username,
            configuration.Password,
            GetOptionalIntEnv("STNEXT_MESSAGEBUS_PUBLISH_TIMEOUT_SECONDS"),
            GetOptionalIntEnv("STNEXT_MESSAGEBUS_SUBSCRIBE_TIMEOUT_SECONDS"));
    }

    public static MessageBusOptions Create(
        string? provider,
        string? host,
        int? port,
        string? clientId,
        string? topicPrefix,
        string? username,
        string? password,
        int? publishTimeoutSeconds = null,
        int? subscribeTimeoutSeconds = null)
    {
        return CreateCore(
            provider,
            host,
            port,
            clientId,
            topicPrefix,
            username,
            password,
            publishTimeoutSeconds,
            subscribeTimeoutSeconds);
    }

    private static MessageBusOptions CreateCore(
        string? provider,
        string? host,
        int? port,
        string? clientId,
        string? topicPrefix,
        string? username,
        string? password,
        int? publishTimeoutSeconds,
        int? subscribeTimeoutSeconds)
    {
        return new MessageBusOptions
        {
            Provider = string.IsNullOrWhiteSpace(provider) ? "inmemory" : provider,
            Host = Normalize(host),
            Port = port,
            ClientId = Normalize(clientId),
            TopicPrefix = Normalize(topicPrefix),
            Username = Normalize(username),
            Password = Normalize(password),
            PublishTimeoutSeconds = publishTimeoutSeconds ?? 5,
            SubscribeTimeoutSeconds = subscribeTimeoutSeconds ?? 5
        };
    }

    private static int? GetOptionalIntEnv(string variableName)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return int.TryParse(raw.Trim(), out var value)
            ? value
            : null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
