using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorRatedParamsDistributionSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for smoke test: {DbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expected = LoadExpected(connection);
        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorRatedParamsValueDistribution();

        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db rated params distribution smoke test failed: no rows returned.");
        }

        foreach (var row in expected)
        {
            var snapshot = actual.FirstOrDefault(x =>
                string.Equals(x.FieldName, row.FieldName, StringComparison.Ordinal)
                && string.Equals(x.RawValue, row.RawValue, StringComparison.Ordinal));

            if (snapshot is null)
            {
                throw new InvalidOperationException($"stp.db rated params distribution smoke test failed: missing row for {row.FieldName}={Display(row.RawValue)}.");
            }

            if (snapshot.Count != row.Count)
            {
                throw new InvalidOperationException($"stp.db rated params distribution smoke test failed: count mismatch for {row.FieldName}={Display(row.RawValue)}. expected={row.Count}, actual={snapshot.Count}");
            }

            if (!string.Equals(snapshot.NormalizedValue ?? string.Empty, row.NormalizedValue, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db rated params distribution smoke test failed: normalized value mismatch for {row.FieldName}={Display(row.RawValue)}. expected={Display(row.NormalizedValue)}, actual={Display(snapshot.NormalizedValue ?? string.Empty)}");
            }
        }
    }

    private static IReadOnlyList<(string FieldName, string RawValue, int Count, string NormalizedValue)> LoadExpected(SqliteConnection connection)
    {
        return new[]
        {
            BuildExpected(connection, "Duty", string.Empty, string.Empty),
            BuildExpected(connection, "Duty", "0", string.Empty),
            BuildExpected(connection, "Connection", "0", "Y"),
            BuildExpected(connection, "Connection", "1", "Δ"),
            BuildExpected(connection, "Connection", string.Empty, string.Empty)
        };
    }

    private static (string FieldName, string RawValue, int Count, string NormalizedValue) BuildExpected(
        SqliteConnection connection,
        string fieldName,
        string rawValue,
        string normalizedValue)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COUNT(*)
FROM ProductTypes
WHERE COALESCE(CAST(json_extract(RatedParams, '$.{fieldName}') AS TEXT), '') = $rawValue;";
        command.Parameters.AddWithValue("$rawValue", rawValue);
        var count = Convert.ToInt32(command.ExecuteScalar() ?? 0);
        return (fieldName, rawValue, count, normalizedValue);
    }

    private static string Display(string value)
        => string.IsNullOrEmpty(value) ? "<empty>" : value;
}
