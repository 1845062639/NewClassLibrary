using System.Text;
using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class MarkdownTestReportRenderer : ITestReportRenderer
{
    public string Format => "md";

    public string Render(TestReportDocument document)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# Test Report - {document.RecordCode}");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine($"- RecordCode: {document.RecordCode}");
        builder.AppendLine($"- SerialNumber: {document.SerialNumber}");
        builder.AppendLine($"- ProductKind: {document.ProductKind}");
        builder.AppendLine($"- TestKindCode: {document.TestKindCode}");
        builder.AppendLine($"- TestTime: {document.TestTime:O}");
        builder.AppendLine($"- IsValid: {document.IsValid}");
        builder.AppendLine($"- OwnDepartment: {document.OwnDepartment ?? string.Empty}");
        builder.AppendLine($"- TestDepartment: {document.TestDepartment ?? string.Empty}");
        builder.AppendLine($"- Tester: {document.Tester ?? string.Empty}");
        if (!string.IsNullOrWhiteSpace(document.Remark))
        {
            builder.AppendLine($"- Remark: {document.Remark}");
        }

        builder.AppendLine();
        builder.AppendLine("## Metadata");
        builder.AppendLine($"- ReportFormatVersion: {document.Metadata.ReportFormatVersion}");
        builder.AppendLine($"- SourceBoundary: {document.Metadata.SourceBoundary}");
        builder.AppendLine($"- PayloadStrategy: {document.Metadata.PayloadStrategy}");
        builder.AppendLine($"- GeneratedAt: {document.Metadata.GeneratedAt:O}");

        builder.AppendLine();
        builder.AppendLine("## Statistics");
        builder.AppendLine($"- ItemCount: {document.Statistics.ItemCount}");
        builder.AppendLine($"- TotalSampleCount: {document.Statistics.TotalSampleCount}");
        builder.AppendLine($"- KeyPointSampleCount: {document.Statistics.KeyPointSampleCount}");
        builder.AppendLine($"- ContinuousSampleCount: {document.Statistics.ContinuousSampleCount}");
        if (document.Statistics.ItemCodes.Count > 0)
        {
            builder.AppendLine($"- ItemCodes: {string.Join(", ", document.Statistics.ItemCodes)}");
        }

        AppendProduct(builder, "Product", document.Product);
        AppendProduct(builder, "AccompanyProduct", document.AccompanyProduct);

        builder.AppendLine();
        builder.AppendLine("## Record Attachments");
        if (document.RecordAttachments.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var attachment in document.RecordAttachments)
            {
                builder.AppendLine($"- {attachment.FileName} ({attachment.FileType}) -> {attachment.StorageKey}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Items");
        if (document.Items.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var item in document.Items)
            {
                builder.AppendLine($"### {item.ItemCode}");
                builder.AppendLine($"- TestRecordItemId: {item.TestRecordItemId}");
                builder.AppendLine($"- MethodCode: {item.MethodCode}");
                builder.AppendLine($"- IsValid: {item.IsValid}");
                if (!string.IsNullOrWhiteSpace(item.Remark))
                {
                    builder.AppendLine($"- Remark: {item.Remark}");
                }
                builder.AppendLine("- DataJson:");
                builder.AppendLine("```json");
                builder.AppendLine(item.DataJson);
                builder.AppendLine("```");

                builder.AppendLine("- Attachments:");
                if (item.Attachments.Count == 0)
                {
                    builder.AppendLine("  - None");
                }
                else
                {
                    foreach (var attachment in item.Attachments)
                    {
                        builder.AppendLine($"  - {attachment.FileName} ({attachment.FileType}) -> {attachment.StorageKey}");
                    }
                }

                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static void AppendProduct(StringBuilder builder, string title, TestReportProductDocument? product)
    {
        builder.AppendLine();
        builder.AppendLine($"## {title}");
        if (product is null)
        {
            builder.AppendLine("- None");
            return;
        }

        builder.AppendLine($"- ProductDefinitionId: {product.ProductDefinitionId}");
        builder.AppendLine($"- ProductKind: {product.ProductKind}");
        builder.AppendLine($"- Code: {product.Code}");
        builder.AppendLine($"- Model: {product.Model}");
        builder.AppendLine($"- Manufacturer: {product.Manufacturer ?? string.Empty}");
        if (!string.IsNullOrWhiteSpace(product.Remark))
        {
            builder.AppendLine($"- Remark: {product.Remark}");
        }
        builder.AppendLine("- RatedParamsJson:");
        builder.AppendLine("```json");
        builder.AppendLine(product.RatedParamsJson);
        builder.AppendLine("```");
    }
}
