namespace StandardTestNext.App.Application.Services;

public sealed class AppStartupOptions
{
    public string DeviceId { get; init; } = "mock-motor-device";
    public string ProductKind { get; init; } = "Motor_Y";
    public string SamplingMode { get; init; } = "single";
    public AppQueryGatewayMode QueryGatewayMode { get; init; } = AppQueryGatewayMode.Auto;
    public string? QueryGatewaySqliteDbPath { get; init; }
    public MessageBusConfiguration MessageBus { get; init; } = new();
}
