using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbSnapshotQuerySmokeTests
{
    public static void Run()
    {
        var service = new StpDbSnapshotQueryService();
        var snapshots = service.ListRecentMotorYRecords(3);
        if (snapshots.Count == 0)
        {
            throw new InvalidOperationException("stp.db snapshot query smoke test failed: no Motor_Y records loaded.");
        }

        foreach (var snapshot in snapshots)
        {
            if (string.IsNullOrWhiteSpace(snapshot.Record.Id))
            {
                throw new InvalidOperationException("stp.db snapshot query smoke test failed: record id missing.");
            }

            if (snapshot.ProductType is null)
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} missing linked product type.");
            }

            if (string.IsNullOrWhiteSpace(snapshot.ProductType.Code))
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} product type code missing.");
            }

            if (snapshot.Items.Count == 0)
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} missing Motor_Y items.");
            }

            foreach (var attachment in snapshot.Record.Attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment.Id) || string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} has record attachment with missing id/fileName.");
                }

                if (attachment.Length <= 0)
                {
                    throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} has record attachment {attachment.FileName} with invalid length.");
                }
            }

            if (!snapshot.Items.Any(item => MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(item.Code)))
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} has no core Motor_Y items.");
            }

            var hasAnyCoreTrialPayload = false;
            foreach (var item in snapshot.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.TestRecordId))
                {
                    throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code ?? snapshot.Record.Id} has item with missing id/recordId.");
                }

                var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
                if (MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(item.Code) && payload.SampleCount > 0)
                {
                    hasAnyCoreTrialPayload = true;
                }
            }

            if (!hasAnyCoreTrialPayload)
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code ?? snapshot.Record.Id} has no core Motor_Y payload with sample data.");
            }

            using var ratedParamsDocument = JsonDocument.Parse(snapshot.ProductType.RatedParamsJson);
            if (ratedParamsDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: product type {snapshot.ProductType.Code} rated params is not a json object.");
            }
        }
    }

    public static void RunAttachmentCoverage()
    {
        var service = new StpDbSnapshotQueryService();
        var snapshots = service.ListRecentMotorYRecords(20);
        if (snapshots.Count == 0)
        {
            throw new InvalidOperationException("stp.db attachment snapshot smoke test failed: no Motor_Y records loaded.");
        }

        var recordAttachments = snapshots
            .Where(snapshot => snapshot.Record.Attachments.Count > 0)
            .SelectMany(snapshot => snapshot.Record.Attachments, (snapshot, attachment) => new { snapshot.Record.Code, Attachment = attachment })
            .ToArray();

        var itemsWithAttachments = snapshots
            .SelectMany(snapshot => snapshot.Items, (snapshot, item) => new { snapshot.Record.Code, Item = item })
            .Where(x => x.Item.Attachments.Count > 0)
            .ToArray();

        if (recordAttachments.Length == 0 && itemsWithAttachments.Length == 0)
        {
            throw new InvalidOperationException("stp.db attachment snapshot smoke test failed: no Motor_Y record/item attachments loaded.");
        }

        foreach (var entry in recordAttachments)
        {
            var attachment = entry.Attachment;
            if (string.IsNullOrWhiteSpace(attachment.Id) || string.IsNullOrWhiteSpace(attachment.FileName))
            {
                throw new InvalidOperationException($"stp.db attachment snapshot smoke test failed: record {entry.Code} has record attachment with missing id/fileName.");
            }

            if (attachment.Length <= 0)
            {
                throw new InvalidOperationException($"stp.db attachment snapshot smoke test failed: record {entry.Code} record attachment {attachment.FileName} length invalid.");
            }
        }

        foreach (var entry in itemsWithAttachments)
        {
            foreach (var attachment in entry.Item.Attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment.Id) || string.IsNullOrWhiteSpace(attachment.FileName))
                {
                    throw new InvalidOperationException($"stp.db attachment snapshot smoke test failed: record {entry.Code} item {entry.Item.Code} has attachment with missing id/fileName.");
                }

                if (attachment.Length <= 0)
                {
                    throw new InvalidOperationException($"stp.db attachment snapshot smoke test failed: record {entry.Code} item {entry.Item.Code} attachment {attachment.FileName} length invalid.");
                }
            }
        }
    }
}
