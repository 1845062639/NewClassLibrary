using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordConsolePresenter
{
    public void PrintSummary(TestRecordAggregate record, TestRecordStatistics? statistics = null, string? reportFormat = null, string? reportContent = null)
    {
        Console.WriteLine($"[Test] Record aggregate built: {record.RecordCode}");
        Console.WriteLine($"[Test] Product: {record.TestProduct?.Model} / {record.ProductKind}");
        Console.WriteLine($"[Test] Item count: {record.Items.Count}, record attachments: {record.Attachments.Count}");
        Console.WriteLine($"[Test] Item codes: {string.Join(", ", record.Items.Select(x => x.ItemCode))}");
        var detailItems = record.Items
            .Select(item =>
            {
                var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
                return new TestRecordItemDetailContract
                {
                    ItemCode = item.ItemCode,
                    DisplayName = item.ItemCode,
                    SortOrder = 0,
                    MethodCode = item.MethodCode,
                    RecordMode = payload.RecordMode ?? string.Empty,
                    SampleCount = payload.SampleCount,
                    LegacySampleCount = payload.LegacySampleCount,
                    HasLegacyPayload = payload.HasLegacyPayload,
                    LegacyPayload = new TestRecordLegacyPayloadContract
                    {
                        LegacySampleCount = payload.LegacyPayload.LegacySampleCount,
                        HasLegacyPayload = payload.LegacyPayload.HasLegacyPayload,
                        PowerCurveImageCount = payload.LegacyPayload.PowerCurveImageCount,
                        TempCurveImageCount = payload.LegacyPayload.TempCurveImageCount,
                        VibrationCurveImageCount = payload.LegacyPayload.VibrationCurveImageCount,
                        HasIncomingPowerMetrics = payload.LegacyPayload.HasIncomingPowerMetrics,
                        HasWindingTemperatureMetrics = payload.LegacyPayload.HasWindingTemperatureMetrics
                    },
                    AttachmentCount = item.Attachments.Count,
                    IsValid = item.IsValid,
                    HasRemark = !string.IsNullOrWhiteSpace(item.Remark),
                    Remark = item.Remark
                };
            })
            .ToArray();
        var legacySummary = TestRecordLegacyPayloadFormatter.FormatDetailSummary(detailItems);
        Console.WriteLine($"[Test] Legacy payload summary: {legacySummary}");

        if (statistics is not null)
        {
            Console.WriteLine($"[Test] Sample stats: total={statistics.TotalSampleCount}, keyPoints={statistics.KeyPointSampleCount}, continuous={statistics.ContinuousSampleCount}");
        }

        if (!string.IsNullOrWhiteSpace(reportFormat))
        {
            Console.WriteLine($"[Test] Report renderer ready: {reportFormat}");
        }

        if (!string.IsNullOrWhiteSpace(reportContent))
        {
            var previewLines = reportContent
                .Split(Environment.NewLine, StringSplitOptions.None)
                .Take(8);
            Console.WriteLine("[Test] Report preview:");
            foreach (var line in previewLines)
            {
                Console.WriteLine(line);
            }
        }
    }
}
