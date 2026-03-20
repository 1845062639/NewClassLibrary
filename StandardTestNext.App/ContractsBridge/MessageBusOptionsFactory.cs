namespace StandardTestNext.App.ContractsBridge;

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
            configuration.Password);
    }

    public static MessageBusOptions Create(
        string? provider,
        string? host,
        int? port,
        string? clientId,
        string? topicPrefix,
        string? username,
        string? password)
    {
        return CreateCore(
            provider,
            host,
            port,
            clientId,
            topicPrefix,
            username,
            password);
    }

    private static MessageBusOptions CreateCore(
        string? provider,
        string? host,
        int? port,
        string? clientId,
        string? topicPrefix,
        string? username,
        string? password)
    {
        return new MessageBusOptions
        {
            Provider = string.IsNullOrWhiteSpace(provider) ? "inmemory" : provider,
            Host = Normalize(host),
            Port = port,
            ClientId = Normalize(clientId),
            TopicPrefix = Normalize(topicPrefix),
            Username = Normalize(username),
            Password = Normalize(password)
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
