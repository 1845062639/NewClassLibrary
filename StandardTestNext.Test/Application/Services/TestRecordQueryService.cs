using System.Text.Json;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordQueryService : ITestRecordQueryService
{
    private readonly ITestRecordRepository _recordRepository;
    private readonly IRecordAttachmentRepository _attachmentRepository;
    private readonly ITestReportRepository _reportRepository;

    public TestRecordQueryService(
        ITestRecordRepository recordRepository,
        IRecordAttachmentRepository attachmentRepository,
        ITestReportRepository reportRepository)
    {
        _recordRepository = recordRepository;
        _attachmentRepository = attachmentRepository;
        _reportRepository = reportRepository;
    }

    public async Task<TestRecordDetail?> GetByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var record = await _recordRepository.FindByRecordCodeAsync(recordCode, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var recordAttachments = await _attachmentRepository.ListForRecordAsync(record.TestRecordId, cancellationToken);
        var reportSnapshots = await _reportRepository.ListForRecordCodeAsync(record.RecordCode, cancellationToken);
        var reportSummaries = reportSnapshots
            .Select(x => new TestReportPersistenceSummary
            {
                RecordCode = x.RecordCode,
                Format = x.Format,
                ExportedAt = x.SavedAt,
                ContentLength = x.Content.Length
            })
            .OrderByDescending(x => x.ExportedAt)
            .ToArray();
        var itemAttachments = new Dictionary<Guid, IReadOnlyList<RecordAttachment>>();
        var itemDetails = new List<TestRecordItemDetail>();

        foreach (var item in record.Items)
        {
            var attachments = await _attachmentRepository.ListForRecordItemAsync(item.TestRecordItemId, cancellationToken);
            itemAttachments[item.TestRecordItemId] = attachments;
            itemDetails.Add(BuildItemDetail(item, attachments));
        }

        return new TestRecordDetail
        {
            Record = record,
            RecordAttachments = recordAttachments,
            ItemAttachments = itemAttachments,
            ItemDetails = itemDetails,
            Reports = reportSnapshots,
            ReportSummaries = reportSummaries
        };
    }

    public async Task<IReadOnlyList<TestRecordSummary>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var records = await _recordRepository.ListRecentAsync(take, cancellationToken);
        var summaries = new List<TestRecordSummary>(records.Count);

        foreach (var record in records)
        {
            var reports = await _reportRepository.ListForRecordCodeAsync(record.RecordCode, cancellationToken);
            var latestReport = reports
                .OrderByDescending(x => x.SavedAt)
                .FirstOrDefault();

            summaries.Add(new TestRecordSummary
            {
                RecordCode = record.RecordCode,
                ProductKind = record.ProductKind,
                ProductCode = record.TestProduct?.Code,
                ProductModel = record.TestProduct?.Model,
                ReusedProductDefinition = record.TestProduct is not null,
                TestKindCode = record.TestKindCode,
                TestTime = record.TestTime,
                ItemCount = record.Items.Count,
                RecordAttachmentCount = record.Attachments.Count,
                ReportCount = reports.Count,
                HasReportArtifacts = reports.Any(x => !string.IsNullOrWhiteSpace(x.ArtifactSavedPath)),
                LatestReportSavedAt = latestReport?.SavedAt
            });
        }

        return summaries;
    }

    private static TestRecordItemDetail BuildItemDetail(TestRecordItemAggregate item, IReadOnlyList<RecordAttachment> attachments)
    {
        var payload = TryParsePayload(item.DataJson);
        return new TestRecordItemDetail
        {
            TestRecordItemId = item.TestRecordItemId,
            ItemCode = item.ItemCode,
            MethodCode = item.MethodCode,
            IsValid = item.IsValid,
            Remark = item.Remark,
            HasRemark = !string.IsNullOrWhiteSpace(item.Remark),
            AttachmentCount = attachments.Count,
            SampleCount = payload.SampleCount,
            RecordMode = payload.RecordMode
        };
    }

    private static (int SampleCount, string? RecordMode) TryParsePayload(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return (0, null);
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            var root = document.RootElement;
            var sampleCount = root.TryGetProperty("SampleCount", out var sampleCountElement) && sampleCountElement.ValueKind == JsonValueKind.Number
                ? sampleCountElement.GetInt32()
                : 0;
            var recordMode = root.TryGetProperty("RecordMode", out var recordModeElement) && recordModeElement.ValueKind == JsonValueKind.String
                ? recordModeElement.GetString()
                : null;
            return (sampleCount, recordMode);
        }
        catch (JsonException)
        {
            return (0, null);
        }
    }
}
