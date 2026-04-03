using System.Text.Json;
using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class ManifestTestReportRenderer : ITestReportRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly TestReportManifestMapper _manifestMapper = new();

    public string Format => "manifest.json";

    public string Render(TestReportDocument document)
    {
        var manifest = new TestReportManifest
        {
            RecordCode = document.RecordCode,
            SerialNumber = document.SerialNumber,
            ProductKind = document.ProductKind,
            TestKindCode = document.TestKindCode,
            TestTime = document.TestTime,
            IsValid = document.IsValid,
            ProductCode = document.Product?.Code,
            ProductModel = document.Product?.Model,
            Tester = document.Tester,
            OwnDepartment = document.OwnDepartment,
            TestDepartment = document.TestDepartment,
            Remark = document.Remark,
            Statistics = new TestReportManifestStatistics
            {
                ItemCount = document.Statistics.ItemCount,
                TotalSampleCount = document.Statistics.TotalSampleCount,
                KeyPointSampleCount = document.Statistics.KeyPointSampleCount,
                ContinuousSampleCount = document.Statistics.ContinuousSampleCount
            }
        };

        foreach (var item in document.Items)
        {
            var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
            manifest.Items.Add(new TestReportManifestItem
            {
                ItemCode = item.ItemCode,
                MethodCode = item.MethodCode,
                RecordMode = payload.RecordMode ?? string.Empty,
                SampleCount = payload.SampleCount,
                IsValid = item.IsValid,
                HasRemark = !string.IsNullOrWhiteSpace(item.Remark)
            });
        }

        return JsonSerializer.Serialize(manifest, SerializerOptions);
    }
}
