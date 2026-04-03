using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYFieldMappingSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    private static readonly IReadOnlyDictionary<string, string[]> RequiredRootFieldsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new[] { "Ruv", "Rvw", "Rwu", "R1c", "θ1c", "ΔR", "R", "Connection" },
            [MotorYTestMethodCodes.NoLoad] = new[] { "Un", "R1c", "θ1c", "K1", "Order", "DecimalPlaces", "DataList", "P0", "I0", "ΔI0", "Pcu", "Pfw", "Pfe", "θ0", "R0", "CoefficientOfPfe", "RConverseType" },
            [MotorYTestMethodCodes.HeatRun] = new[] { "Rc", "θc", "K1", "Rn", "θb", "Time", "θs", "Order", "DecimalPlaces", "Data1List", "Data2List" },
            [MotorYTestMethodCodes.LoadA] = new[] { "Un", "Pn", "R1c", "θ1c", "K1", "θa", "ΔT", "Order", "PolePairs", "DecimalPlaces", "RawDataList", "ResultDataList" },
            [MotorYTestMethodCodes.LoadB] = new[] { "Un", "Pn", "R1c", "θ1c", "K1", "K2", "θb", "θs", "θw", "Order", "PolePairs", "ΔT", "A", "B", "R", "DecimalPlaces", "Pfw", "Pcu1", "Pcu2", "Ps", "RawDataList", "ResultDataList" },
            [MotorYTestMethodCodes.LockedRotor] = new[] { "Un", "In", "Tn", "PolePairs", "CoefficientOfPfe", "K1", "R1c", "θ1c", "Order", "DecimalPlaces", "DataList", "Ikn", "IknDivideIn", "Pkn", "Tkn", "TknDivideTn" }
        };

    private static readonly IReadOnlyDictionary<string, string[]> OptionalRootFieldsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.HeatRun] = new[] { "θw", "Rw", "Δθ" },
            [MotorYTestMethodCodes.LoadA] = new[] { "CoefficientOfPfe", "CorrectionType", "K2", "Pfw" },
            [MotorYTestMethodCodes.LoadB] = new[] { "CoefficientOfPfe", "TorqueCorrection", "IsTorqueModify", "θ1tChanelSelect", "θaChanelSelect" },
            [MotorYTestMethodCodes.LockedRotor] = new[] { "TorqueCalType", "RCalType", "C1", "θw", "θb", "R1s" }
        };

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for smoke test: {DbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        foreach (var canonicalCode in RequiredRootFieldsByCanonicalCode.Keys)
        {
            var sampleJson = FindBestBaselinePayload(connection, canonicalCode);
            using var document = JsonDocument.Parse(sampleJson);
            var root = document.RootElement;
            var missingFields = RequiredRootFieldsByCanonicalCode[canonicalCode]
                .Where(field => !root.TryGetProperty(field, out _))
                .ToArray();

            if (missingFields.Length > 0)
            {
                throw new InvalidOperationException($"stp.db field mapping smoke test failed for {canonicalCode}. Missing fields: {string.Join(", ", missingFields)}");
            }

            if (OptionalRootFieldsByCanonicalCode.TryGetValue(canonicalCode, out var optionalFields))
            {
                var presentOptionalFieldCount = optionalFields.Count(field => root.TryGetProperty(field, out _));
                if (presentOptionalFieldCount == 0)
                {
                    throw new InvalidOperationException($"stp.db field mapping smoke test failed for {canonicalCode}. Optional/derived field group unexpectedly all absent: {string.Join(", ", optionalFields)}");
                }
            }
        }
    }

    private static string FindBestBaselinePayload(SqliteConnection connection, string canonicalCode)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Data
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
ORDER BY rowid DESC;";

        string? fallbackJson = null;
        var bestScore = -1;
        string? bestJson = null;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var dataJson = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            if (MotorYLegacyItemCodeNormalizer.Normalize(legacyCode) != canonicalCode || string.IsNullOrWhiteSpace(dataJson))
            {
                continue;
            }

            var payload = TestRecordItemPayloadReader.TryParse(dataJson);
            if (payload.SampleCount <= 0)
            {
                continue;
            }

            fallbackJson ??= dataJson;

            using var document = JsonDocument.Parse(dataJson);
            var root = document.RootElement;
            var score = 0;
            if (OptionalRootFieldsByCanonicalCode.TryGetValue(canonicalCode, out var optionalFields))
            {
                score = optionalFields.Count(field => root.TryGetProperty(field, out _));
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestJson = dataJson;
            }
        }

        return bestJson ?? fallbackJson ?? throw new InvalidOperationException($"stp.db field mapping smoke test missing viable payload for {canonicalCode}.");
    }
}
