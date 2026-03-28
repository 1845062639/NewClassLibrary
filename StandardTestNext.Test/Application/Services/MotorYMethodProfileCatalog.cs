namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 显式沉淀旧 Motor_Y 业务项在 stp.db 中出现的 Method 口径，
/// 供 next-gen 查询/适配层识别“同一业务项下的不同旧方法分支”。
/// </summary>
public sealed class MotorYMethodProfile
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int MethodValue { get; init; }
    public string ProfileKey { get; init; } = string.Empty;
    public string LegacyAlgorithmEntry { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsBaselineEnumValue { get; init; }
}

public static class MotorYMethodProfileCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<int, MotorYMethodProfile>> Profiles =
        new Dictionary<string, IReadOnlyDictionary<int, MotorYMethodProfile>>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = BuildProfiles(
                MotorYTestMethodCodes.DcResistance,
                "Algorithm_Motor_Y 直流电阻流程外部依赖同一 payload 形状（TestData_Motor_Y_Direct_Current_Resistance）",
                1,
                (1, "baseline", "旧 TestMethodEnum.Motor_Y_Direct_Current_Resistance，对应主基线直流电阻测定", true),
                (35, "delivery", "旧现场变体：出厂/交付态直流电阻测定", false),
                (53, "companion", "旧现场变体：陪试/伴随机直流电阻测定", false),
                (54, "delivery-companion", "旧现场变体：出厂+陪试直流电阻测定", false)),

            [MotorYTestMethodCodes.NoLoad] = BuildProfiles(
                MotorYTestMethodCodes.NoLoad,
                nameof(MotorYLegacyAlgorithmEntrypoints.NoLoad),
                0,
                (0, "baseline", "旧 TestMethodEnum.Motor_Y_NoLoad，对应空载特性试验基线", true),
                (59, "delivery", "旧现场变体：空载试验（出厂/交付态）", false)),

            [MotorYTestMethodCodes.HeatRun] = BuildProfiles(
                MotorYTestMethodCodes.HeatRun,
                nameof(MotorYLegacyAlgorithmEntrypoints.Thermal),
                3,
                (3, "baseline", "旧 TestMethodEnum.Motor_Y_Thermal，对应热试验基线", true),
                (47, "companion", "旧现场变体：陪试/伴随机热试验", false),
                (48, "other-variant", "旧现场少量变体：热试验扩展方法号", false)),

            [MotorYTestMethodCodes.LoadA] = BuildProfiles(
                MotorYTestMethodCodes.LoadA,
                nameof(MotorYLegacyAlgorithmEntrypoints.LoadA),
                4,
                (4, "baseline", "旧 TestMethodEnum.Motor_Y_Load_A，对应 A 法负载试验基线", true),
                (60, "delivery", "旧现场主流变体：A 法负载试验（出厂/交付态）", false),
                (61, "other-variant", "旧现场少量变体：A 法负载试验扩展方法号", false)),

            [MotorYTestMethodCodes.LoadB] = BuildProfiles(
                MotorYTestMethodCodes.LoadB,
                nameof(MotorYLegacyAlgorithmEntrypoints.LoadB),
                5,
                (5, "baseline", "旧 TestMethodEnum.Motor_Y_Load_B，对应 B 法负载试验基线", true),
                (51, "delivery", "旧现场主流变体：B 法负载试验（出厂/交付态）", false),
                (52, "other-variant", "旧现场少量变体：B 法负载试验扩展方法号", false)),

            [MotorYTestMethodCodes.LockedRotor] = BuildProfiles(
                MotorYTestMethodCodes.LockedRotor,
                nameof(MotorYLegacyAlgorithmEntrypoints.LockRotor),
                11,
                (11, "baseline", "旧 TestMethodEnum.Motor_Y_Lock_Rotor，对应堵转特性试验基线", true),
                (46, "delivery", "旧现场变体：堵转试验（出厂/交付态）", false),
                (47, "legacy-alias", "旧现场变体：堵转试验历史方法号，需与热试验 47 结合业务项区分", false))
        };

    public static MotorYMethodProfile? TryGet(string? canonicalCode, int? methodValue)
    {
        if (string.IsNullOrWhiteSpace(canonicalCode) || !methodValue.HasValue)
        {
            return null;
        }

        return Profiles.TryGetValue(canonicalCode, out var methods)
            && methods.TryGetValue(methodValue.Value, out var profile)
            ? profile
            : null;
    }

    public static IReadOnlyCollection<int> GetKnownMethods(string canonicalCode)
    {
        return Profiles.TryGetValue(canonicalCode, out var methods)
            ? methods.Keys.ToArray()
            : Array.Empty<int>();
    }

    private static IReadOnlyDictionary<int, MotorYMethodProfile> BuildProfiles(
        string canonicalCode,
        string legacyAlgorithmEntry,
        int baselineMethodValue,
        params (int methodValue, string profileKey, string description, bool? isBaseline)[] entries)
    {
        var result = new Dictionary<int, MotorYMethodProfile>();
        foreach (var entry in entries)
        {
            result[entry.methodValue] = new MotorYMethodProfile
            {
                CanonicalCode = canonicalCode,
                MethodValue = entry.methodValue,
                ProfileKey = entry.profileKey,
                LegacyAlgorithmEntry = legacyAlgorithmEntry,
                Description = entry.description,
                IsBaselineEnumValue = entry.isBaseline ?? entry.methodValue == baselineMethodValue
            };
        }

        return result;
    }
}

public static class MotorYLegacyAlgorithmEntrypoints
{
    public const string NoLoad = "Algorithm_Motor_Y.NoLoad";
    public const string Thermal = "Algorithm_Motor_Y.Thermal";
    public const string LoadA = "Algorithm_Motor_Y.Load_A";
    public const string LoadB = "Algorithm_Motor_Y.Load_B";
    public const string LockRotor = "Algorithm_Motor_Y.Lock_Rotor";
}
