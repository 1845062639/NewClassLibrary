using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYPayloadSmokeTests
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

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Method, Data
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

        var verified = new HashSet<string>(StringComparer.Ordinal);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var method = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            var dataJson = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(legacyCode) || verified.Contains(canonicalCode))
            {
                continue;
            }

            var payload = TestRecordItemPayloadReader.TryParse(dataJson);
            if (!CanUseAsBaselineSample(canonicalCode, payload))
            {
                continue;
            }

            AssertPayload(canonicalCode, method, payload, dataJson);
            verified.Add(canonicalCode);
        }

        var expected = new[]
        {
            MotorYTestMethodCodes.DcResistance,
            MotorYTestMethodCodes.NoLoad,
            MotorYTestMethodCodes.HeatRun,
            MotorYTestMethodCodes.LoadA,
            MotorYTestMethodCodes.LoadB,
            MotorYTestMethodCodes.LockedRotor
        };

        var missing = expected.Where(code => !verified.Contains(code)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y smoke test missing items: {string.Join(", ", missing)}");
        }
    }

    private static bool CanUseAsBaselineSample(string canonicalCode, TestRecordItemPayloadSnapshot payload)
    {
        return canonicalCode switch
        {
            var code when code == MotorYTestMethodCodes.DcResistance => payload.SampleCount > 0,
            var code when code == MotorYTestMethodCodes.NoLoad => payload.SampleCount > 0 && payload.RecordMode == TestRecordSampleModes.KeyPointOnly,
            var code when code == MotorYTestMethodCodes.HeatRun => payload.SampleCount > 0 && payload.RecordMode == TestRecordSampleModes.Continuous,
            var code when code == MotorYTestMethodCodes.LoadA => payload.SampleCount > 0 && payload.RecordMode == TestRecordSampleModes.Continuous,
            var code when code == MotorYTestMethodCodes.LoadB => payload.SampleCount > 0 && payload.RecordMode == TestRecordSampleModes.Continuous,
            var code when code == MotorYTestMethodCodes.LockedRotor => payload.SampleCount > 0 && payload.RecordMode == TestRecordSampleModes.KeyPointOnly,
            _ => false
        };
    }

    private static bool MatchesMethod(int method, params int[] expectedMethods)
    {
        foreach (var expected in expectedMethods)
        {
            if (method == expected)
            {
                return true;
            }
        }

        return false;
    }

    private static void AssertPayload(
        string canonicalCode,
        int method,
        TestRecordItemPayloadSnapshot payload,
        string dataJson)
    {
        if (payload.SampleCount <= 0)
        {
            throw new InvalidOperationException($"stp.db payload smoke test failed for {canonicalCode}: sample count should be > 0.");
        }

        switch (canonicalCode)
        {
            case var code when code == MotorYTestMethodCodes.DcResistance:
                if (!string.IsNullOrWhiteSpace(payload.RecordMode))
                {
                    throw new InvalidOperationException($"DC resistance should not infer record mode, got '{payload.RecordMode}'.");
                }

                if (!dataJson.Contains("Ruv", StringComparison.Ordinal) || method != 1)
                {
                    throw new InvalidOperationException("DC resistance payload/method shape mismatch against stp.db baseline.");
                }
                break;

            case var code when code == MotorYTestMethodCodes.NoLoad:
                if (payload.RecordMode != TestRecordSampleModes.KeyPointOnly || !MatchesMethod(method, 0, 2))
                {
                    throw new InvalidOperationException($"No-load payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
                }
                break;

            case var code when code == MotorYTestMethodCodes.HeatRun:
                if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 3))
                {
                    throw new InvalidOperationException($"Heat-run payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
                }
                break;

            case var code when code == MotorYTestMethodCodes.LoadA:
                if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 4, 60))
                {
                    throw new InvalidOperationException($"Load-A payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
                }
                break;

            case var code when code == MotorYTestMethodCodes.LoadB:
                if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 5))
                {
                    throw new InvalidOperationException($"Load-B payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
                }
                break;

            case var code when code == MotorYTestMethodCodes.LockedRotor:
                if (payload.RecordMode != TestRecordSampleModes.KeyPointOnly || !MatchesMethod(method, 11, 47))
                {
                    throw new InvalidOperationException($"Locked-rotor payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
                }
                break;
        }
    }
}
