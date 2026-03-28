using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodDistributionSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method distribution smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var distribution = service.ListMotorYMethodDistribution();
        if (distribution.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method distribution smoke test failed: no distribution rows loaded.");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expectedRows = LoadExpectedRows(connection);
        foreach (var expected in expectedRows)
        {
            var actual = distribution.FirstOrDefault(row =>
                string.Equals(row.CanonicalCode, expected.CanonicalCode, StringComparison.Ordinal)
                && row.Method == expected.Method);

            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: missing row for {expected.CanonicalCode}:{expected.Method}.");
            }

            if (actual.Count != expected.Count)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: count mismatch for {expected.CanonicalCode}:{expected.Method}. expected={expected.Count}, actual={actual.Count}");
            }

            if (!string.Equals(actual.MethodKey, $"{expected.CanonicalCode}:{expected.Method}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: method key mismatch for {expected.CanonicalCode}:{expected.Method}.");
            }

            var route = MotorYLegacyAlgorithmRouteResolver.Resolve(expected.CanonicalCode, expected.Method);
            if (!string.Equals(actual.MethodProfileKey, route?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(actual.VariantKind, route?.VariantKind, StringComparison.Ordinal)
                || !string.Equals(actual.AlgorithmFamily, route?.AlgorithmFamily, StringComparison.Ordinal)
                || !string.Equals(actual.LegacyEnumName, route?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(actual.LegacyFormName, route?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(actual.LegacyAlgorithmEntry, route?.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(actual.LegacyMethodName, route?.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(actual.LegacySettingsMethodName, route?.LegacySettingsMethodName, StringComparison.Ordinal)
                || actual.IsBaselineMethod != (route?.IsBaselineMethod == true))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: route projection mismatch for {expected.CanonicalCode}:{expected.Method}.");
            }
        }

        AssertTopCount(distribution, MotorYTestMethodCodes.DcResistance, 1, 431);
        AssertTopCount(distribution, MotorYTestMethodCodes.NoLoad, 0, 31);
        AssertTopCount(distribution, MotorYTestMethodCodes.HeatRun, 3, 430);
        AssertTopCount(distribution, MotorYTestMethodCodes.LoadA, 60, 61);
        AssertTopCount(distribution, MotorYTestMethodCodes.LoadB, 5, 233);
        AssertTopCount(distribution, MotorYTestMethodCodes.LockedRotor, 46, 1);
    }

    private static IReadOnlyList<(string CanonicalCode, int Method, int Count)> LoadExpectedRows(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Method, COUNT(*)
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
  AND Method IS NOT NULL
GROUP BY Code, Method;";

        var rows = new List<(string CanonicalCode, int Method, int Count)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(reader.GetString(0));
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            rows.Add((canonicalCode, reader.GetInt32(1), reader.GetInt32(2)));
        }

        return rows;
    }

    private static void AssertTopCount(
        IReadOnlyList<StpDbMotorYMethodDistributionSnapshot> distribution,
        string canonicalCode,
        int expectedMethod,
        int expectedCount)
    {
        var top = distribution
            .Where(row => string.Equals(row.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Method)
            .FirstOrDefault();

        if (top is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: no rows for {canonicalCode}.");
        }

        if (top.Method != expectedMethod || top.Count != expectedCount)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: top distribution mismatch for {canonicalCode}. expected={expectedMethod}/{expectedCount}, actual={top.Method}/{top.Count}");
        }
    }
}
