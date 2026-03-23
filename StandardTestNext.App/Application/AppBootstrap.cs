using StandardTestNext.App.ContractsBridge;
using StandardTestNext.App.Devices;
using StandardTestNext.App.Application.Services;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public sealed class AppBootstrap
{
    private readonly ITestRecordQueryGateway? _testRecordGateway;

    public AppBootstrap(ITestRecordQueryGateway? testRecordGateway = null)
    {
        _testRecordGateway = testRecordGateway;
    }

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

        var testRecordGateway = TestRecordQueryGatewayFactory.Create(() => _testRecordGateway);
        var recentRecords = testRecordGateway.ListRecentAsync(5).GetAwaiter().GetResult();
        var latestRecord = recentRecords.FirstOrDefault();
        if (latestRecord is not null)
        {
            Console.WriteLine($"[App] Test record query gateway preview: {latestRecord.RecordCode}:{latestRecord.ProductDisplayName}:attachments={latestRecord.RecordAttachmentCount}/{latestRecord.ItemAttachmentBucketCount}:reports={latestRecord.ReportCount}:primary={latestRecord.PrimaryReportArtifactFileName ?? "<none>"}:light={latestRecord.LightweightReportArtifactFileName ?? "<none>"}");
            if (latestRecord.ItemPartitions.Count > 0)
            {
                Console.WriteLine($"[App] List partitions: {string.Join(", ", latestRecord.ItemPartitions.Select(x => $"{x.DisplayName}:{x.RecordMode}:{x.SampleCount}:legacy={x.LegacySampleCount}:payload={(x.HasLegacyPayload ? "Y" : "N")}"))}");
            }

            var listLegacySummary = TestRecordLegacyPayloadFormatter.FormatListSummary(latestRecord.ItemPartitions);
            if (!string.IsNullOrWhiteSpace(listLegacySummary))
            {
                Console.WriteLine($"[App] List legacy payload summary: {listLegacySummary}");
            }

            var detail = testRecordGateway.GetDetailAsync(latestRecord.RecordCode).GetAwaiter().GetResult();
            if (detail is not null)
            {
                Console.WriteLine($"[App] Detail preview: items={detail.ItemCount}, samples={detail.SampleCount}, keyPoints={detail.KeyPointSampleCount}, continuous={detail.ContinuousSampleCount}, reports={detail.ReportSummaries.Count}");
                if (detail.ItemDetails.Count > 0)
                {
                    Console.WriteLine($"[App] Detail items: {string.Join(", ", detail.ItemDetails.Select(x => $"{x.DisplayName}:{x.RecordMode}:{x.SampleCount}:legacy={x.LegacySampleCount}:payload={(x.HasLegacyPayload ? "Y" : "N")}:attachments={x.AttachmentCount}"))}");
                    Console.WriteLine($"[App] Detail legacy payload summary: {TestRecordLegacyPayloadFormatter.FormatDetailSummary(detail.ItemDetails)}");
                }

                var primaryReport = detail.ReportSummaries.FirstOrDefault(x => x.IsPrimaryEntry);
                if (primaryReport is not null)
                {
                    Console.WriteLine($"[App] Primary report artifact: {primaryReport.ArtifactFileName ?? "<none>"} @ {primaryReport.ArtifactSavedPath ?? "<none>"}");
                }
            }
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
