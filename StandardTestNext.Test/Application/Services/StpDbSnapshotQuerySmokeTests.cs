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
            if (string.IsNullOrWhiteSpace(snapshot.Record.Id) || string.IsNullOrWhiteSpace(snapshot.Record.Code))
            {
                throw new InvalidOperationException("stp.db snapshot query smoke test failed: record id/code missing.");
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

            if (!snapshot.Items.Any(item => MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(item.Code)))
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} has no core Motor_Y items.");
            }

            foreach (var item in snapshot.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.TestRecordId))
                {
                    throw new InvalidOperationException($"stp.db snapshot query smoke test failed: record {snapshot.Record.Code} has item with missing id/recordId.");
                }

                var payload = TestRecordItemPayloadReader.TryParse(item.DataJson);
                if (MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(item.Code) && payload.SampleCount <= 0)
                {
                    throw new InvalidOperationException($"stp.db snapshot query smoke test failed: item {item.Code} sample count invalid.");
                }
            }

            using var ratedParamsDocument = JsonDocument.Parse(snapshot.ProductType.RatedParamsJson);
            if (ratedParamsDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"stp.db snapshot query smoke test failed: product type {snapshot.ProductType.Code} rated params is not a json object.");
            }
        }
    }
}
