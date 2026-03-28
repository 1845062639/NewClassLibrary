using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYPayloadSmokeTests
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

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        AssertMethodCoverage(connection);

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

            if (!IsValidBaselineCandidate(canonicalCode, dataJson))
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

    private static void AssertMethodCoverage(SqliteConnection connection)
    {
        var actualMethodsByCanonicalCode = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);

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

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var method = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            if (!actualMethodsByCanonicalCode.TryGetValue(canonicalCode, out var methods))
            {
                methods = new HashSet<int>();
                actualMethodsByCanonicalCode[canonicalCode] = methods;
            }

            methods.Add(method);
        }

        foreach (var (canonicalCode, expectedMethods) in ExpectedMethodsByCanonicalCode)
        {
            if (!actualMethodsByCanonicalCode.TryGetValue(canonicalCode, out var actualMethods))
            {
                throw new InvalidOperationException($"stp.db method coverage missing canonical item '{canonicalCode}'.");
            }

            var missingMethods = expectedMethods.Where(expected => !actualMethods.Contains(expected)).ToArray();
            if (missingMethods.Length > 0)
            {
                throw new InvalidOperationException(
                    $"stp.db method coverage mismatch for {canonicalCode}. Missing methods: {string.Join(", ", missingMethods)}. Actual methods: {string.Join(", ", actualMethods.OrderBy(x => x))}");
            }
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

    private static bool IsValidBaselineCandidate(string canonicalCode, string dataJson)
    {
        switch (canonicalCode)
        {
            case var code when code == MotorYTestMethodCodes.DcResistance:
                return dataJson.Contains("\"Ruv\"", StringComparison.Ordinal);

            case var code when code == MotorYTestMethodCodes.NoLoad:
                var noLoad = MotorYNoLoadLegacyShape.FromJson(dataJson);
                return noLoad is not null && noLoad.DataList.Count > 0 && (noLoad.P0 > 0 || noLoad.I0 > 0 || noLoad.DataList.Any(x => x.P0 > 0 || x.I0 > 0));

            case var code when code == MotorYTestMethodCodes.HeatRun:
                var heatRun = MotorYThermalLegacyShape.FromJson(dataJson);
                return heatRun is not null
                    && heatRun.Data1List.Count > 0
                    && heatRun.Data2List.Count > 0
                    && (heatRun.Δθ > 0 || heatRun.θw > 0 || heatRun.Rw > 0 || heatRun.Data1List.Any(x => x.θ1 > 0 || x.P1 > 0) || heatRun.Data2List.Any(x => x.R > 0));

            case var code when code == MotorYTestMethodCodes.LoadA:
                var loadA = MotorYLoadALegacyShape.FromJson(dataJson);
                return loadA is not null
                    && loadA.RawDataList.Count > 0
                    && loadA.ResultDataList.Count > 0
                    && (loadA.Un > 0 || loadA.Pn > 0 || loadA.RawDataList.Any(x => x.U > 0 || x.I1 > 0 || x.P1t > 0));

            case var code when code == MotorYTestMethodCodes.LoadB:
                var loadB = MotorYLoadBLegacyShape.FromJson(dataJson);
                return loadB is not null
                    && loadB.RawDataList.Count > 0
                    && loadB.ResultDataList.Count > 0
                    && (loadB.Pfw > 0 || loadB.Pcu1 > 0 || loadB.Pcu2 > 0 || loadB.RawDataList.Any(x => x.U > 0 || x.I1 > 0 || x.P1t > 0));

            case var code when code == MotorYTestMethodCodes.LockedRotor:
                var lockedRotor = MotorYLockRotorLegacyShape.FromJson(dataJson);
                return lockedRotor is not null
                    && lockedRotor.DataList.Count > 0
                    && (lockedRotor.Ikn > 0 || lockedRotor.Pkn > 0 || lockedRotor.DataList.Any(x => x.Uk > 0 || x.Ik > 0 || x.Pk > 0 || x.Tk > 0));

            default:
                return false;
        }
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
                AssertDcResistance(method, payload, dataJson);
                break;

            case var code when code == MotorYTestMethodCodes.NoLoad:
                AssertNoLoad(method, payload, dataJson);
                break;

            case var code when code == MotorYTestMethodCodes.HeatRun:
                AssertHeatRun(method, payload, dataJson);
                break;

            case var code when code == MotorYTestMethodCodes.LoadA:
                AssertLoadA(method, payload, dataJson);
                break;

            case var code when code == MotorYTestMethodCodes.LoadB:
                AssertLoadB(method, payload, dataJson);
                break;

            case var code when code == MotorYTestMethodCodes.LockedRotor:
                AssertLockedRotor(method, payload, dataJson);
                break;
        }
    }

    private static void AssertDcResistance(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (!string.IsNullOrWhiteSpace(payload.RecordMode))
        {
            throw new InvalidOperationException($"DC resistance should not infer record mode, got '{payload.RecordMode}'.");
        }

        if (!dataJson.Contains("Ruv", StringComparison.Ordinal) || !MatchesMethod(method, 1, 35, 53, 54))
        {
            throw new InvalidOperationException("DC resistance payload/method shape mismatch against stp.db baseline.");
        }
    }

    private static void AssertNoLoad(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (payload.RecordMode != TestRecordSampleModes.KeyPointOnly || !MatchesMethod(method, 0, 59))
        {
            throw new InvalidOperationException($"No-load payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
        }

        var shape = MotorYNoLoadLegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("No-load payload cannot deserialize to MotorYNoLoadLegacyShape.");

        if (shape.DataList.Count <= 0)
        {
            throw new InvalidOperationException("No-load legacy shape should contain DataList baseline samples.");
        }

        if (shape.P0 <= 0 || shape.I0 <= 0)
        {
            throw new InvalidOperationException($"No-load baseline summary invalid. P0={shape.P0}, I0={shape.I0}");
        }
    }

    private static void AssertHeatRun(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 3, 47, 48))
        {
            throw new InvalidOperationException($"Heat-run payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
        }

        var shape = MotorYThermalLegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("Heat-run payload cannot deserialize to MotorYThermalLegacyShape.");

        if (shape.Data1List.Count <= 0 || shape.Data2List.Count <= 0)
        {
            throw new InvalidOperationException("Heat-run legacy shape should contain Data1List + Data2List baseline samples.");
        }

        if (shape.Δθ <= 0 && shape.θw <= 0 && shape.Rw <= 0)
        {
            throw new InvalidOperationException($"Heat-run baseline summary invalid. Δθ={shape.Δθ}, θw={shape.θw}, Rw={shape.Rw}");
        }
    }

    private static void AssertLoadA(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 4, 60))
        {
            throw new InvalidOperationException($"Load-A payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
        }

        var shape = MotorYLoadALegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("Load-A payload cannot deserialize to MotorYLoadALegacyShape.");

        if (shape.RawDataList.Count <= 0 || shape.ResultDataList.Count <= 0)
        {
            throw new InvalidOperationException("Load-A legacy shape should contain RawDataList + ResultDataList baseline samples.");
        }

        if (shape.Un <= 0 || shape.Pn <= 0)
        {
            throw new InvalidOperationException($"Load-A baseline rated summary invalid. Un={shape.Un}, Pn={shape.Pn}");
        }
    }

    private static void AssertLoadB(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (payload.RecordMode != TestRecordSampleModes.Continuous || !MatchesMethod(method, 5, 51, 52))
        {
            throw new InvalidOperationException($"Load-B payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
        }

        var shape = MotorYLoadBLegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("Load-B payload cannot deserialize to MotorYLoadBLegacyShape.");

        if (shape.RawDataList.Count <= 0 || shape.ResultDataList.Count <= 0)
        {
            throw new InvalidOperationException("Load-B legacy shape should contain RawDataList + ResultDataList baseline samples.");
        }

        if (shape.Pfw < 0 || shape.Pcu1 < 0 || shape.Pcu2 < 0)
        {
            throw new InvalidOperationException($"Load-B baseline loss summary invalid. Pfw={shape.Pfw}, Pcu1={shape.Pcu1}, Pcu2={shape.Pcu2}");
        }
    }

    private static void AssertLockedRotor(int method, TestRecordItemPayloadSnapshot payload, string dataJson)
    {
        if (payload.RecordMode != TestRecordSampleModes.KeyPointOnly || !MatchesMethod(method, 11, 46, 47))
        {
            throw new InvalidOperationException($"Locked-rotor payload/method mismatch. recordMode={payload.RecordMode}, method={method}");
        }

        var shape = MotorYLockRotorLegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("Locked-rotor payload cannot deserialize to MotorYLockRotorLegacyShape.");

        if (shape.DataList.Count <= 0)
        {
            throw new InvalidOperationException("Locked-rotor legacy shape should contain DataList baseline samples.");
        }

        var first = shape.DataList[0];
        if (shape.Ikn <= 0 && shape.Pkn <= 0 && first.Ik <= 0 && first.Pk <= 0)
        {
            throw new InvalidOperationException($"Locked-rotor baseline summary invalid. Ikn={shape.Ikn}, Pkn={shape.Pkn}, Ik={first.Ik}, Pk={first.Pk}");
        }
    }
}
