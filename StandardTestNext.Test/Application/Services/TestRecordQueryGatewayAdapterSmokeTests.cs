using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.AppSide;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Domain.Records;
using StandardTestNext.Test.Infrastructure.Persistence;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordQueryGatewayAdapterSmokeTests
{
    public static void Run()
    {
        ShouldExposeLegacyPayloadSummaryThroughAppQueryGateway();
    }

    private static void ShouldExposeLegacyPayloadSummaryThroughAppQueryGateway()
    {
        var now = DateTimeOffset.Parse("2026-03-23T08:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = now,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = "Noise",
                    MethodCode = "M-Noise",
                    IsValid = true,
                    DataJson = """
                    {
                      "SampleCount": 3,
                      "LegacySampleCount": 2,
                      "RecordMode": "legacy-replay",
                      "LegacySamples": [
                        {
                          "LeaveFactoryModePowerCurveImage": "power-a.png",
                          "LeaveFactoryModeTempCurveImage": "temp-a.png",
                          "LeaveFactoryModeVibrationCurveImage": "vibration-a.png",
                          "UabIncoming": 380.5,
                          "Temp1": 88.2
                        },
                        {
                          "TempRiseModePowerCurveImage": "power-b.png",
                          "TempRiseModeTempCurveImage": "temp-b.png",
                          "TempRiseModeVibrationFrequencyCurveImage": "vibration-b.png"
                        }
                      ]
                    }
                    """
                }
            }
        };

        var recordRepository = new InMemoryTestRecordRepository();
        var attachmentRepository = new InMemoryRecordAttachmentRepository();
        var reportRepository = new InMemoryTestReportRepository();
        recordRepository.SaveAsync(record).GetAwaiter().GetResult();

        var queryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
        var facade = new TestRecordQueryFacade(queryService);
        var gateway = new TestRecordQueryGatewayAdapter(facade);

        var list = gateway.ListRecentAsync(5).GetAwaiter().GetResult();
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();

        var listItem = list.Single();
        var partition = listItem.ItemPartitions.Single();
        if (partition.ItemCode != "Noise"
            || partition.SampleCount != 3
            || partition.LegacySampleCount != 2
            || !partition.HasLegacyPayload)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter list smoke test failed.");
        }

        if (detail is null)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter detail smoke test returned null.");
        }

        var item = detail.ItemDetails.Single();
        if (item.ItemCode != "Noise"
            || item.SampleCount != 3
            || item.LegacySampleCount != 2
            || !item.HasLegacyPayload
            || item.LegacyPayload.PowerCurveImageCount != 2
            || item.LegacyPayload.TempCurveImageCount != 2
            || item.LegacyPayload.VibrationCurveImageCount != 2
            || !item.LegacyPayload.HasIncomingPowerMetrics
            || !item.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter detail smoke test failed.");
        }

        var summary = TestRecordLegacyPayloadFormatter.FormatDetailSummary(detail.ItemDetails);
        const string expected = "Noise:legacy=2:power=2:temp=2:vibration=2:incoming=Y:winding=Y";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestRecordQueryGatewayAdapter summary smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }
}
