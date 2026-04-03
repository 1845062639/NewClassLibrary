namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 供 next-gen 构造/持久化链路直接携带的 Motor_Y 旧方法路由元数据，
/// 让 demo/builder 生成的记录也能保持与 stp.db 查询快照一致的旧系统语义。
/// </summary>
public sealed class MotorYTrialItemBuildProfile
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int MethodValue { get; init; }
    public string MethodKey { get; init; } = string.Empty;
    public string ProfileKey { get; init; } = string.Empty;
    public string VariantKind { get; init; } = string.Empty;
    public string AlgorithmFamily { get; init; } = string.Empty;
    public string LegacyEnumName { get; init; } = string.Empty;
    public string LegacyFormName { get; init; } = string.Empty;
    public string LegacyAlgorithmEntry { get; init; } = string.Empty;
    public string LegacyMethodName { get; init; } = string.Empty;
    public string LegacySettingsMethodName { get; init; } = string.Empty;
    public bool IsBaselineMethod { get; init; }

    public static MotorYTrialItemBuildProfile FromRoute(MotorYLegacyAlgorithmRoute route)
    {
        return new MotorYTrialItemBuildProfile
        {
            CanonicalCode = route.CanonicalCode,
            MethodValue = route.MethodValue,
            MethodKey = route.MethodKey,
            ProfileKey = route.ProfileKey,
            VariantKind = route.VariantKind,
            AlgorithmFamily = route.AlgorithmFamily,
            LegacyEnumName = route.LegacyEnumName,
            LegacyFormName = route.LegacyFormName,
            LegacyAlgorithmEntry = route.LegacyAlgorithmEntry,
            LegacyMethodName = route.LegacyMethodName,
            LegacySettingsMethodName = route.LegacySettingsMethodName,
            IsBaselineMethod = route.IsBaselineMethod
        };
    }
}
