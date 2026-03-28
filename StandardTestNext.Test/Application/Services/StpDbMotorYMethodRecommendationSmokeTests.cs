using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodRecommendationSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method recommendation smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYMethodRecommendations();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method recommendation smoke test failed: no rows returned.");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expected = new[]
        {
            BuildExpected(connection, MotorYTestMethodCodes.DcResistance, 1),
            BuildExpected(connection, MotorYTestMethodCodes.NoLoad, 0),
            BuildExpected(connection, MotorYTestMethodCodes.HeatRun, 3),
            BuildExpected(connection, MotorYTestMethodCodes.LoadA, 4),
            BuildExpected(connection, MotorYTestMethodCodes.LoadB, 5),
            BuildExpected(connection, MotorYTestMethodCodes.LockedRotor, 11)
        };

        foreach (var row in expected)
        {
            var snapshot = actual.FirstOrDefault(x => string.Equals(x.CanonicalCode, row.CanonicalCode, StringComparison.Ordinal));
            if (snapshot is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: missing row for {row.CanonicalCode}.");
            }

            if (snapshot.TotalCount != row.TotalCount
                || snapshot.BaselineMethod != row.BaselineMethod
                || snapshot.BaselineCount != row.BaselineCount
                || snapshot.DominantMethod != row.DominantMethod
                || snapshot.DominantCount != row.DominantCount)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: numeric mismatch for {row.CanonicalCode}.");
            }

            if (!string.Equals(snapshot.BaselineMethodKey, $"{row.CanonicalCode}:{row.BaselineMethod}", StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantMethodKey, $"{row.CanonicalCode}:{row.DominantMethod}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: method key mismatch for {row.CanonicalCode}.");
            }

            var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.DominantMethod);
            if (!string.Equals(snapshot.DominantProfileKey, dominantRoute?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantVariantKind, dominantRoute?.VariantKind, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantAlgorithmFamily, dominantRoute?.AlgorithmFamily, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyEnumName, dominantRoute?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyFormName, dominantRoute?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyAlgorithmEntry, dominantRoute?.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyMethodName, dominantRoute?.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacySettingsMethodName, dominantRoute?.LegacySettingsMethodName, StringComparison.Ordinal)
                || snapshot.DominantIsBaselineMethod != (dominantRoute?.IsBaselineMethod == true))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: dominant route projection mismatch for {row.CanonicalCode}.");
            }

            var expectedShare = row.TotalCount <= 0
                ? 0d
                : Math.Round((double)row.DominantCount / row.TotalCount, 4, MidpointRounding.AwayFromZero);
            if (Math.Abs(snapshot.DominantShare - expectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: dominant share mismatch for {row.CanonicalCode}. expected={expectedShare}, actual={snapshot.DominantShare}");
            }

            var shouldPrioritizeDominant = row.BaselineMethod != row.DominantMethod
                && expectedShare >= DominantOverrideThreshold;
            if (snapshot.ShouldPrioritizeDominantOverBaseline != shouldPrioritizeDominant)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: prioritize flag mismatch for {row.CanonicalCode}.");
            }
        }
    }

    private static (string CanonicalCode, int TotalCount, int BaselineMethod, int BaselineCount, int DominantMethod, int DominantCount) BuildExpected(
        SqliteConnection connection,
        string canonicalCode,
        int baselineMethod)
    {
        var rows = LoadCounts(connection, canonicalCode);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: no source rows for {canonicalCode}.");
        }

        var baselineCount = rows.TryGetValue(baselineMethod, out var baseline) ? baseline : 0;
        var dominant = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .First();

        return (canonicalCode, rows.Values.Sum(), baselineMethod, baselineCount, dominant.Key, dominant.Value);
    }

    private static Dictionary<int, int> LoadCounts(SqliteConnection connection, string canonicalCode)
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

        var result = new Dictionary<int, int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var normalized = MotorYLegacyItemCodeNormalizer.Normalize(reader.GetString(0));
            if (!string.Equals(normalized, canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            var method = reader.GetInt32(1);
            var count = reader.GetInt32(2);
            result[method] = result.TryGetValue(method, out var current)
                ? current + count
                : count;
        }

        return result;
    }
}
