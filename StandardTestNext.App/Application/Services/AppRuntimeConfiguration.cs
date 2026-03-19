namespace StandardTestNext.App.Application.Services;

public sealed class AppRuntimeConfiguration
{
    public string DeviceId { get; init; } = "mock-motor-device";
    public string ProductKind { get; init; } = "Motor_Y";
    public string SamplingMode { get; init; } = "single";
    public string MessageBusProvider { get; init; } = "inmemory";

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
