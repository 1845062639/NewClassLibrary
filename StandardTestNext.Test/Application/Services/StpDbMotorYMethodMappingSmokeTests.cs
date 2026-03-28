using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodMappingSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    private static readonly IReadOnlyDictionary<string, int[]> ExpectedMethodsByCanonicalCode =
        new Dictionary<string, int[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new[] { 1, 35, 53, 54 },
            [MotorYTestMethodCodes.NoLoad] = new[] { 0, 59 },
            [MotorYTestMethodCodes.HeatRun] = new[] { 3, 47, 48 },
            [MotorYTestMethodCodes.LoadA] = new[] { 4, 60, 61 },
            [MotorYTestMethodCodes.LoadB] = new[] { 5, 51, 52 },
            [MotorYTestMethodCodes.LockedRotor] = new[] { 11, 46, 47 }
        };

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var snapshots = service.ListRecentMotorYRecords(20);
        if (snapshots.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method mapping smoke test failed: no records loaded.");
        }

        var allItems = snapshots.SelectMany(snapshot => snapshot.Items).ToArray();
        if (allItems.Length == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method mapping smoke test failed: no Motor_Y items loaded.");
        }

        foreach (var item in allItems)
        {
            var expectedCanonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(item.Code);
            if (!string.Equals(item.CanonicalCode, expectedCanonicalCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method mapping smoke test failed: item {item.Id} canonical code mismatch. expected={expectedCanonicalCode}, actual={item.CanonicalCode}");
            }

            var expectedMethodKey = item.Method.HasValue
                ? $"{expectedCanonicalCode}:{item.Method.Value}"
                : $"{expectedCanonicalCode}:<null>";
            if (!string.Equals(item.MethodKey, expectedMethodKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method mapping smoke test failed: item {item.Id} method key mismatch. expected={expectedMethodKey}, actual={item.MethodKey}");
            }
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        foreach (var pair in ExpectedMethodsByCanonicalCode)
        {
            var actualMethods = LoadMethods(connection, pair.Key);
            var missing = pair.Value.Except(actualMethods).ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method mapping smoke test failed for {pair.Key}. Missing methods: {string.Join(", ", missing)}");
            }
        }
    }

    private static IReadOnlyCollection<int> LoadMethods(SqliteConnection connection, string canonicalCode)
    {
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
    '堵转特性试验');";

        var methods = new HashSet<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            if (!string.Equals(MotorYLegacyItemCodeNormalizer.Normalize(legacyCode), canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            if (!reader.IsDBNull(1))
            {
                methods.Add(reader.GetInt32(1));
            }
        }

        return methods;
    }
}
