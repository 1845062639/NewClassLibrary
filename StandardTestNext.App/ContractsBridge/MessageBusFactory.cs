namespace StandardTestNext.App.ContractsBridge;

public static class MessageBusFactory
{
    public static IMessageBus Create(MessageBusOptions? options = null)
    {
        var provider = NormalizeProvider(options?.Provider);

        return provider switch
        {
            "inmemory" => new InMemoryMessageBus(),
            "mqtt" => new MqttMessageBus(options ?? new MessageBusOptions { Provider = "mqtt" }),
            _ => throw new NotSupportedException($"Message bus provider '{provider}' is not supported yet.")
        };
    }

    private static string NormalizeProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "inmemory"
            : provider.Trim().ToLowerInvariant();
    }
}
