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
            SamplingMode = SamplingMode,
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
