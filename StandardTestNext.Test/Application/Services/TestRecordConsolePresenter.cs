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
