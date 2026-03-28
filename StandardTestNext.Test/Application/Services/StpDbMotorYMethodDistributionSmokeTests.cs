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

        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.DcResistance);
        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.NoLoad);
        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.HeatRun);
        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.LoadA);
        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.LoadB);
        AssertTopCount(distribution, expectedRows, MotorYTestMethodCodes.LockedRotor);
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

        var grouped = new Dictionary<(string CanonicalCode, int Method), int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(reader.GetString(0));
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            var key = (canonicalCode, reader.GetInt32(1));
            grouped[key] = grouped.TryGetValue(key, out var count)
                ? count + reader.GetInt32(2)
                : reader.GetInt32(2);
        }

        return grouped
            .Select(pair => (pair.Key.CanonicalCode, pair.Key.Method, pair.Value))
            .OrderBy(row => row.CanonicalCode, StringComparer.Ordinal)
            .ThenBy(row => row.Method)
            .ToArray();
    }

    private static void AssertTopCount(
        IReadOnlyList<StpDbMotorYMethodDistributionSnapshot> distribution,
        IReadOnlyList<(string CanonicalCode, int Method, int Count)> expectedRows,
        string canonicalCode)
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

        var expectedTop = expectedRows
            .Where(row => string.Equals(row.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Method)
            .FirstOrDefault();

        if (expectedTop == default)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: no expected rows for {canonicalCode}.");
        }

        if (top.Method != expectedTop.Method || top.Count != expectedTop.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method distribution smoke test failed: top distribution mismatch for {canonicalCode}. expected={expectedTop.Method}/{expectedTop.Count}, actual={top.Method}/{top.Count}");
        }
    }
}
