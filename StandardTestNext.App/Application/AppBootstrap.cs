using StandardTestNext.App.ContractsBridge;
using StandardTestNext.App.Devices;
using StandardTestNext.App.Application.Services;

namespace StandardTestNext.App.Application;

public sealed class AppBootstrap
{
    public void Run(IMessageBus messageBus, AppStartupOptions? options = null)
    {
        Console.WriteLine("StandardTestNext.App starting...");

        options ??= new AppStartupOptions();
        var normalizedSamplingMode = NormalizeSamplingMode(options.SamplingMode);

        Console.WriteLine($"[App] Device: {options.DeviceId}, Product: {options.ProductKind}, SamplingMode: {normalizedSamplingMode}");

        var deviceGateway = new MockMotorDeviceGateway(options.DeviceId, options.ProductKind);
        var sampleService = new MotorSamplingService(deviceGateway, messageBus);
        var statusService = new DeviceStatusReportingService(messageBus);
        var commandConsumer = new AppCommandConsumer(messageBus, statusService, options.DeviceId);

        commandConsumer.Start();
        statusService.ReportReady(options.DeviceId, options.ProductKind);

        if (string.Equals(normalizedSamplingMode, "burst", StringComparison.OrdinalIgnoreCase))
        {
            sampleService.PublishSample();
            sampleService.PublishSample();
        }
        else
        {
            sampleService.PublishSample();
        }

        var testRecordGateway = new TestRecordQueryGatewayStub();
        var recentRecords = testRecordGateway.ListRecentAsync(5).GetAwaiter().GetResult();
        var latestRecord = recentRecords.FirstOrDefault();
        if (latestRecord is not null)
        {
            Console.WriteLine($"[App] Test record query gateway preview: {latestRecord.RecordCode}:{latestRecord.ProductDisplayName}:reports={latestRecord.ReportCount}:primary={latestRecord.PrimaryReportArtifactFileName ?? "<none>"}");
        }

        Console.WriteLine("StandardTestNext.App ready.");
    }

    private static string NormalizeSamplingMode(string? samplingMode)
    {
        return string.IsNullOrWhiteSpace(samplingMode)
            ? "single"
            : samplingMode.Trim().ToLowerInvariant();
    }
}
