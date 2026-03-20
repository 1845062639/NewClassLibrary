using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestReportDocumentMapper
{
    public TestReportDocument Map(TestRecordAggregate record, TestRecordStatistics? statistics = null)
    {
        var document = new TestReportDocument
        {
            RecordCode = record.RecordCode,
            SerialNumber = record.SerialNumber,
            ProductKind = record.ProductKind,
            TestKindCode = record.TestKindCode,
            TestTime = record.TestTime,
            IsValid = record.IsValid,
            OwnDepartment = record.OwnDepartment,
            TestDepartment = record.TestDepartment,
            Tester = record.Tester,
            Remark = record.Remark,
            Metadata = new TestReportMetadataDocument
            {
                ReportFormatVersion = "v1",
                SourceBoundary = "StandardTestNext.Test",
                PayloadStrategy = "Phase-1 JSON payload",
                GeneratedAt = DateTimeOffset.Now
            },
            Statistics = MapStatistics(record, statistics),
            Product = MapProduct(record.TestProduct),
            AccompanyProduct = MapProduct(record.AccompanyProduct)
        };

        foreach (var attachment in record.Attachments)
        {
            document.RecordAttachments.Add(MapAttachment(attachment));
        }

        foreach (var item in record.Items)
        {
            var itemDocument = new TestReportItemDocument
            {
                TestRecordItemId = item.TestRecordItemId.ToString(),
                ItemCode = item.ItemCode,
                MethodCode = item.MethodCode,
                DataJson = item.DataJson,
                Remark = item.Remark,
                IsValid = item.IsValid
            };

            foreach (var attachment in item.Attachments)
            {
                itemDocument.Attachments.Add(MapAttachment(attachment));
            }

            document.Items.Add(itemDocument);
        }

        return document;
    }

    private static TestReportStatisticsDocument MapStatistics(TestRecordAggregate record, TestRecordStatistics? statistics)
    {
        if (statistics is null)
        {
            return new TestReportStatisticsDocument
            {
                ItemCount = record.Items.Count,
                TotalSampleCount = 0,
                KeyPointSampleCount = 0,
                ContinuousSampleCount = 0
            };
        }

        var document = new TestReportStatisticsDocument
        {
            ItemCount = statistics.ItemCount,
            TotalSampleCount = statistics.TotalSampleCount,
            KeyPointSampleCount = statistics.KeyPointSampleCount,
            ContinuousSampleCount = statistics.ContinuousSampleCount
        };

        foreach (var itemCode in statistics.ItemCodes)
        {
            document.ItemCodes.Add(itemCode);
        }

        return document;
    }

    private static TestReportProductDocument? MapProduct(ProductDefinition? product)
    {
        if (product is null)
        {
            return null;
        }

        return new TestReportProductDocument
        {
            ProductDefinitionId = product.ProductId.ToString(),
            ProductKind = product.ProductKind,
            Code = product.Code,
            Model = product.Model,
            Manufacturer = product.Manufacturer,
            RatedParamsJson = product.RatedParamsJson,
            Remark = product.Remark
        };
    }

    private static TestReportAttachmentDocument MapAttachment(RecordAttachment attachment)
    {
        return new TestReportAttachmentDocument
        {
            AttachmentId = attachment.AttachmentId.ToString(),
            FileName = attachment.FileName,
            FileType = attachment.FileType,
            StorageKey = attachment.StorageKey,
            Remark = attachment.Remark
        };
    }
}
