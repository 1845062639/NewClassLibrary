using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYLegacyCodeDistributionSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for legacy code distribution smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYLegacyCodeDistribution();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y legacy code distribution smoke test failed: no rows loaded.");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expected = LoadExpectedRows(connection);
        foreach (var row in expected)
        {
            var match = actual.FirstOrDefault(x =>
                string.Equals(x.CanonicalCode, row.CanonicalCode, StringComparison.Ordinal)
                && string.Equals(x.LegacyCode, row.LegacyCode, StringComparison.Ordinal)
                && x.Method == row.Method);

            if (match is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y legacy code distribution smoke test failed: missing row {row.CanonicalCode}/{row.LegacyCode}/{row.Method}.");
            }

            if (match.Count != row.Count)
            {
                throw new InvalidOperationException($"stp.db Motor_Y legacy code distribution smoke test failed: count mismatch for {row.CanonicalCode}/{row.LegacyCode}/{row.Method}. expected={row.Count}, actual={match.Count}");
            }

            var expectedMethodKey = row.Method.HasValue ? $"{row.CanonicalCode}:{row.Method.Value}" : $"{row.CanonicalCode}:null";
            if (!string.Equals(match.MethodKey, expectedMethodKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y legacy code distribution smoke test failed: method key mismatch for {row.CanonicalCode}/{row.LegacyCode}/{row.Method}.");
            }

            var expectedRoute = row.Method.HasValue
                ? MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.Method.Value)
                : null;
            if (!string.Equals(match.Route?.ProfileKey, expectedRoute?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(match.Route?.LegacyFormName, expectedRoute?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(match.Route?.LegacyEnumName, expectedRoute?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(match.Route?.AlgorithmFamily, expectedRoute?.AlgorithmFamily, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y legacy code distribution smoke test failed: route projection mismatch for {row.CanonicalCode}/{row.LegacyCode}/{row.Method}.");
            }
        }

        AssertAlias(actual, MotorYTestMethodCodes.NoLoad, "空载特性试验", 0, 351);
        AssertAlias(actual, MotorYTestMethodCodes.NoLoad, "空载试验", 59, 13);
        AssertAlias(actual, MotorYTestMethodCodes.LockedRotor, "堵转特性试验", 11, 5);
        AssertAlias(actual, MotorYTestMethodCodes.LockedRotor, "堵转试验", 46, 1);
    }

    private static IReadOnlyList<(string CanonicalCode, string LegacyCode, int? Method, int Count)> LoadExpectedRows(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COALESCE(Code, ''), Method, COUNT(*)
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
GROUP BY COALESCE(Code, ''), Method;";

        var rows = new List<(string CanonicalCode, string LegacyCode, int? Method, int Count)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            rows.Add((canonicalCode, legacyCode, reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1), reader.GetInt32(2)));
        }

        return rows;
    }

    private static void AssertAlias(
        IReadOnlyList<StpDbMotorYLegacyCodeDistributionSnapshot> rows,
        string canonicalCode,
        string legacyCode,
        int method,
        int expectedCount)
    {
        var row = rows.FirstOrDefault(x =>
            string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal)
            && string.Equals(x.LegacyCode, legacyCode, StringComparison.Ordinal)
            && x.Method == method);

        if (row is null || row.Count != expectedCount)
        {
            throw new InvalidOperationException($"stp.db Motor_Y legacy code distribution smoke test failed: expected alias {canonicalCode}/{legacyCode}/{method}={expectedCount}, actual={(row is null ? "<missing>" : row.Count.ToString())}");
        }
    }
}
