namespace StandardTestNext.App.Application.Services;

public sealed class AppRuntimeConfiguration
{
    public string DeviceId { get; init; } = "mock-motor-device";
    public string ProductKind { get; init; } = "Motor_Y";
    public string SamplingMode { get; init; } = "single";
    public MessageBusConfiguration MessageBus { get; init; } = new();

    public string MessageBusProvider => MessageBus.Provider;

    public AppStartupOptions ToStartupOptions()
    {
        return new AppStartupOptions
        {
            DeviceId = DeviceId,
            ProductKind = ProductKind,
            SamplingMode = SamplingMode
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
