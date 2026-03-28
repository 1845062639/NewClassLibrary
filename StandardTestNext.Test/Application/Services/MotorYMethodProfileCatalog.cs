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
    public string VariantKind { get; init; } = string.Empty;
    public string AlgorithmFamily { get; init; } = string.Empty;
    public string LegacyEnumName { get; init; } = string.Empty;
    public string LegacyFormName { get; init; } = string.Empty;
    public string LegacyAlgorithmEntry { get; init; } = string.Empty;
    public string LegacyMethodName { get; init; } = string.Empty;
    public string LegacySettingsMethodName { get; init; } = string.Empty;
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
                MotorYLegacyAlgorithmEntrypoints.DcResistance,
                MotorYLegacyMethodNames.DcResistance,
                MotorYSettingsMethodNames.DcResistance,
                1,
                (1, "baseline", MotorYLegacyEnumNames.DcResistance, MotorYLegacyFormNames.DcResistance, "旧 TestMethodEnum.Motor_Y_Direct_Current_Resistance，对应主基线直流电阻测定", true),
                (35, "delivery", MotorYLegacyEnumNames.DcResistanceDelivery, MotorYLegacyFormNames.DcResistance, "旧现场变体：出厂/交付态直流电阻测定", false),
                (53, "companion", MotorYLegacyEnumNames.DcResistanceCompanion, MotorYLegacyFormNames.DcResistanceCompanion, "旧现场变体：陪试/伴随机直流电阻测定", false),
                (54, "delivery-companion", MotorYLegacyEnumNames.DcResistanceDeliveryCompanion, MotorYLegacyFormNames.DcResistanceCompanion, "旧现场变体：出厂+陪试直流电阻测定", false)),

            [MotorYTestMethodCodes.NoLoad] = BuildProfiles(
                MotorYTestMethodCodes.NoLoad,
                MotorYLegacyAlgorithmEntrypoints.NoLoad,
                MotorYLegacyMethodNames.NoLoad,
                MotorYSettingsMethodNames.NoLoad,
                0,
                (0, "baseline", MotorYLegacyEnumNames.NoLoad, MotorYLegacyFormNames.NoLoad, "旧 TestMethodEnum.Motor_Y_NoLoad，对应空载特性试验基线", true),
                (59, "delivery", MotorYLegacyEnumNames.NoLoadDelivery, MotorYLegacyFormNames.NoLoadDelivery, "旧现场变体：空载试验（出厂/交付态）", false)),

            [MotorYTestMethodCodes.HeatRun] = BuildProfiles(
                MotorYTestMethodCodes.HeatRun,
                MotorYLegacyAlgorithmEntrypoints.Thermal,
                MotorYLegacyMethodNames.Thermal,
                MotorYSettingsMethodNames.Thermal,
                3,
                (3, "baseline", MotorYLegacyEnumNames.Thermal, MotorYLegacyFormNames.Thermal, "旧 TestMethodEnum.Motor_Y_Thermal，对应热试验基线", true),
                (47, "companion", MotorYLegacyEnumNames.ThermalCompanion, MotorYLegacyFormNames.ThermalCompanion, "旧现场变体：陪试/伴随机热试验", false),
                (48, "other-variant", MotorYLegacyEnumNames.ThermalCydj, MotorYLegacyFormNames.ThermalCydj, "旧现场少量变体：热试验扩展方法号", false)),

            [MotorYTestMethodCodes.LoadA] = BuildProfiles(
                MotorYTestMethodCodes.LoadA,
                MotorYLegacyAlgorithmEntrypoints.LoadA,
                MotorYLegacyMethodNames.LoadA,
                MotorYSettingsMethodNames.LoadA,
                4,
                (4, "baseline", MotorYLegacyEnumNames.LoadA, MotorYLegacyFormNames.LoadA, "旧 TestMethodEnum.Motor_Y_Load_A，对应 A 法负载试验基线", true),
                (60, "delivery", MotorYLegacyEnumNames.LoadADelivery, MotorYLegacyFormNames.LoadA, "旧现场主流变体：A 法负载试验（出厂/交付态）", false),
                (61, "other-variant", MotorYLegacyEnumNames.LoadAOtherVariant, MotorYLegacyFormNames.LoadA, "旧现场少量变体：A 法负载试验扩展方法号", false)),

            [MotorYTestMethodCodes.LoadB] = BuildProfiles(
                MotorYTestMethodCodes.LoadB,
                MotorYLegacyAlgorithmEntrypoints.LoadB,
                MotorYLegacyMethodNames.LoadB,
                MotorYSettingsMethodNames.LoadB,
                5,
                (5, "baseline", MotorYLegacyEnumNames.LoadB, MotorYLegacyFormNames.LoadB, "旧 TestMethodEnum.Motor_Y_Load_B，对应 B 法负载试验基线", true),
                (51, "delivery", MotorYLegacyEnumNames.LoadBDelivery, MotorYLegacyFormNames.LoadB, "旧现场主流变体：B 法负载试验（出厂/交付态）", false),
                (52, "other-variant", MotorYLegacyEnumNames.LoadBOtherVariant, MotorYLegacyFormNames.LoadB, "旧现场少量变体：B 法负载试验扩展方法号", false)),

            [MotorYTestMethodCodes.LockedRotor] = BuildProfiles(
                MotorYTestMethodCodes.LockedRotor,
                MotorYLegacyAlgorithmEntrypoints.LockRotor,
                MotorYLegacyMethodNames.LockRotor,
                MotorYSettingsMethodNames.LockRotor,
                11,
                (11, "baseline", MotorYLegacyEnumNames.LockedRotor, MotorYLegacyFormNames.LockedRotor, "旧 TestMethodEnum.Motor_Y_Lock_Rotor，对应堵转特性试验基线", true),
                (46, "delivery", MotorYLegacyEnumNames.LockedRotorDelivery, MotorYLegacyFormNames.LockedRotorDelivery, "旧现场变体：堵转试验（出厂/交付态）", false),
                (47, "legacy-alias", MotorYLegacyEnumNames.LockedRotorLegacyAlias, MotorYLegacyFormNames.LockedRotor, "旧现场变体：堵转试验历史方法号，需与热试验 47 结合业务项区分", false))
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
        string legacyMethodName,
        string legacySettingsMethodName,
        int baselineMethodValue,
        params (int methodValue, string profileKey, string legacyEnumName, string legacyFormName, string description, bool? isBaseline)[] entries)
    {
        var result = new Dictionary<int, MotorYMethodProfile>();
        var algorithmFamily = ResolveAlgorithmFamily(canonicalCode);
        foreach (var entry in entries)
        {
            result[entry.methodValue] = new MotorYMethodProfile
            {
                CanonicalCode = canonicalCode,
                MethodValue = entry.methodValue,
                ProfileKey = entry.profileKey,
                VariantKind = ResolveVariantKind(entry.profileKey),
                AlgorithmFamily = algorithmFamily,
                LegacyEnumName = entry.legacyEnumName,
                LegacyFormName = entry.legacyFormName,
                LegacyAlgorithmEntry = legacyAlgorithmEntry,
                LegacyMethodName = legacyMethodName,
                LegacySettingsMethodName = legacySettingsMethodName,
                Description = entry.description,
                IsBaselineEnumValue = entry.isBaseline ?? entry.methodValue == baselineMethodValue
            };
        }

        return result;
    }

    private static string ResolveVariantKind(string profileKey)
    {
        return profileKey switch
        {
            "baseline" => MotorYLegacyVariantKinds.Baseline,
            "delivery" => MotorYLegacyVariantKinds.Delivery,
            "companion" => MotorYLegacyVariantKinds.Companion,
            "delivery-companion" => MotorYLegacyVariantKinds.DeliveryCompanion,
            "legacy-alias" => MotorYLegacyVariantKinds.LegacyAlias,
            _ => MotorYLegacyVariantKinds.OtherVariant
        };
    }

    private static string ResolveAlgorithmFamily(string canonicalCode)
    {
        return canonicalCode switch
        {
            var code when code == MotorYTestMethodCodes.DcResistance => MotorYLegacyAlgorithmFamilies.DirectCurrentResistance,
            var code when code == MotorYTestMethodCodes.NoLoad => MotorYLegacyAlgorithmFamilies.NoLoad,
            var code when code == MotorYTestMethodCodes.HeatRun => MotorYLegacyAlgorithmFamilies.Thermal,
            var code when code == MotorYTestMethodCodes.LoadA => MotorYLegacyAlgorithmFamilies.LoadA,
            var code when code == MotorYTestMethodCodes.LoadB => MotorYLegacyAlgorithmFamilies.LoadB,
            var code when code == MotorYTestMethodCodes.LockedRotor => MotorYLegacyAlgorithmFamilies.LockedRotor,
            _ => string.Empty
        };
    }
}

public static class MotorYLegacyAlgorithmEntrypoints
{
    public const string DcResistance = "Algorithm_Motor_Y.Direct_Current_Resistance";
    public const string NoLoad = "Algorithm_Motor_Y.NoLoad";
    public const string Thermal = "Algorithm_Motor_Y.Thermal";
    public const string LoadA = "Algorithm_Motor_Y.Load_A";
    public const string LoadB = "Algorithm_Motor_Y.Load_B";
    public const string LockRotor = "Algorithm_Motor_Y.Lock_Rotor";
}

public static class MotorYLegacyMethodNames
{
    public const string DcResistance = "直流电阻测定";
    public const string NoLoad = "空载试验";
    public const string Thermal = "热试验";
    public const string LoadA = "A法负载试验";
    public const string LoadB = "B法负载试验";
    public const string LockRotor = "堵转试验";
}

public static class MotorYSettingsMethodNames
{
    public const string DcResistance = "Motor_Y_Direct_Current_Resistance";
    public const string NoLoad = "Motor_Y_NoLoad";
    public const string Thermal = "Motor_Y_Thermal";
    public const string LoadA = "Motor_Y_Load_A";
    public const string LoadB = "Motor_Y_Load_B";
    public const string LockRotor = "Motor_Y_Lock_Rotor";
}

public static class MotorYLegacyEnumNames
{
    public const string DcResistance = "Motor_Y_Direct_Current_Resistance";
    public const string DcResistanceDelivery = "Motor_Y_Direct_Current_Resistance_DeliveryTest";
    public const string DcResistanceCompanion = "Motor_Y_Direct_Current_Resistance_Companion";
    public const string DcResistanceDeliveryCompanion = "Motor_Y_Direct_Current_Resistance_DeliveryTest_Companion";
    public const string NoLoad = "Motor_Y_NoLoad";
    public const string NoLoadDelivery = "Motor_Y_NoLoad_DeliveryTest";
    public const string Thermal = "Motor_Y_Thermal";
    public const string ThermalCompanion = "Motor_Y_Thermal_Companion";
    public const string ThermalCydj = "Motor_Y_Thermal_CYDJ";
    public const string LoadA = "Motor_Y_Load_A";
    public const string LoadADelivery = "Motor_Y_Load_A_DeliveryTest";
    public const string LoadAOtherVariant = "Motor_Y_Load_A_OtherVariant";
    public const string LoadB = "Motor_Y_Load_B";
    public const string LoadBDelivery = "Motor_Y_Load_B_DeliveryTest";
    public const string LoadBOtherVariant = "Motor_Y_Load_B_OtherVariant";
    public const string LockedRotor = "Motor_Y_Lock_Rotor";
    public const string LockedRotorDelivery = "Motor_Y_Lock_Rotor_DeliveryTest";
    public const string LockedRotorLegacyAlias = "Motor_Y_Lock_Rotor_LegacyAlias";
}

public static class MotorYLegacyFormNames
{
    public const string DcResistance = "FrmMotor_Y_Direct_Current_Resistance";
    public const string DcResistanceCompanion = "FrmMotor_Y_Direct_Current_Resistance_Companion";
    public const string NoLoad = "FrmMotor_Y_NoLoad";
    public const string NoLoadDelivery = "FrmMotor_Y_NoLoad_DeliveryTest";
    public const string Thermal = "FrmMotor_Y_Thermal";
    public const string ThermalCompanion = "FrmMotor_Y_Thermal_Companion";
    public const string ThermalCydj = "FrmMotor_Y_Thermal_CYDJ";
    public const string LoadA = "FrmMotor_Y_Load_A";
    public const string LoadB = "FrmMotor_Y_Load_B";
    public const string LockedRotor = "FrmMotor_Y_Lock_Rotor";
    public const string LockedRotorDelivery = "FrmMotor_Y_Lock_Rotor_DeliveryTest";
}
