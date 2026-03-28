using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYMethodProfileCatalogSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method profile smoke test: {DbPath}");
        }

        ShouldCoverRealMethodValuesFromStpDb();
        ShouldExposeMethodProfileMetadataOnSnapshots();
        ShouldExposeLegacyMethodRoutingNames();
    }

    private static void ShouldCoverRealMethodValuesFromStpDb()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Method
FROM TestRecordItems
WHERE Code IN (
    '直流电阻测定',
    '空载试验',
    '空载特性试验',
    '热试验',
    'A法负载试验',
    'B法负载试验',
    '堵转试验',
    '堵转特性试验')
  AND Method IS NOT NULL;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            var method = reader.GetInt32(1);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            var profile = MotorYMethodProfileCatalog.TryGet(canonicalCode, method);
            if (profile is null)
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: missing profile for {canonicalCode}:{method} (legacy code={legacyCode}).");
            }

            if (!string.Equals(profile.CanonicalCode, canonicalCode, StringComparison.Ordinal)
                || profile.MethodValue != method)
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: profile payload mismatch for {canonicalCode}:{method}.");
            }
        }
    }

    private static void ShouldExposeMethodProfileMetadataOnSnapshots()
    {
        var service = new StpDbSnapshotQueryService(DbPath);
        var snapshots = service.ListRecentMotorYRecords(20);
        var items = snapshots.SelectMany(x => x.Items).Where(x => x.Method.HasValue).ToArray();
        if (items.Length == 0)
        {
            throw new InvalidOperationException("Motor_Y method profile smoke test failed: no snapshot items loaded.");
        }

        foreach (var item in items)
        {
            var profile = MotorYMethodProfileCatalog.TryGet(item.CanonicalCode, item.Method);
            if (profile is null)
            {
                continue;
            }

            if (!string.Equals(item.MethodProfileKey, profile.ProfileKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: item {item.Id} profile key mismatch. expected={profile.ProfileKey}, actual={item.MethodProfileKey}");
            }

            if (!string.Equals(item.LegacyAlgorithmEntry, profile.LegacyAlgorithmEntry, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: item {item.Id} legacy algorithm entry mismatch. expected={profile.LegacyAlgorithmEntry}, actual={item.LegacyAlgorithmEntry}");
            }

            if (!string.Equals(item.LegacyMethodName, profile.LegacyMethodName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: item {item.Id} legacy method name mismatch. expected={profile.LegacyMethodName}, actual={item.LegacyMethodName}");
            }

            if (!string.Equals(item.LegacySettingsMethodName, profile.LegacySettingsMethodName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: item {item.Id} settings method name mismatch. expected={profile.LegacySettingsMethodName}, actual={item.LegacySettingsMethodName}");
            }

            if (item.IsBaselineMethod != profile.IsBaselineEnumValue)
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: item {item.Id} baseline flag mismatch.");
            }
        }
    }

    private static void ShouldExposeLegacyMethodRoutingNames()
    {
        var expected = new (string CanonicalCode, int Method, string AlgorithmEntry, string LegacyMethodName, string SettingsMethodName)[]
        {
            (MotorYTestMethodCodes.DcResistance, 1, MotorYLegacyAlgorithmEntrypoints.DcResistance, MotorYLegacyMethodNames.DcResistance, MotorYSettingsMethodNames.DcResistance),
            (MotorYTestMethodCodes.NoLoad, 0, MotorYLegacyAlgorithmEntrypoints.NoLoad, MotorYLegacyMethodNames.NoLoad, MotorYSettingsMethodNames.NoLoad),
            (MotorYTestMethodCodes.HeatRun, 3, MotorYLegacyAlgorithmEntrypoints.Thermal, MotorYLegacyMethodNames.Thermal, MotorYSettingsMethodNames.Thermal),
            (MotorYTestMethodCodes.LoadA, 4, MotorYLegacyAlgorithmEntrypoints.LoadA, MotorYLegacyMethodNames.LoadA, MotorYSettingsMethodNames.LoadA),
            (MotorYTestMethodCodes.LoadB, 5, MotorYLegacyAlgorithmEntrypoints.LoadB, MotorYLegacyMethodNames.LoadB, MotorYSettingsMethodNames.LoadB),
            (MotorYTestMethodCodes.LockedRotor, 11, MotorYLegacyAlgorithmEntrypoints.LockRotor, MotorYLegacyMethodNames.LockRotor, MotorYSettingsMethodNames.LockRotor)
        };

        foreach (var row in expected)
        {
            var profile = MotorYMethodProfileCatalog.TryGet(row.CanonicalCode, row.Method)
                ?? throw new InvalidOperationException($"Motor_Y method profile smoke test failed: missing baseline profile for {row.CanonicalCode}:{row.Method}.");

            if (!string.Equals(profile.LegacyAlgorithmEntry, row.AlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(profile.LegacyMethodName, row.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(profile.LegacySettingsMethodName, row.SettingsMethodName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y method profile smoke test failed: baseline routing name mismatch for {row.CanonicalCode}:{row.Method}.");
            }
        }
    }
}
