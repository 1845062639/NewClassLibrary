namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 供后续 Motor_Y 适配层直接消费的结构化旧算法路由信息，
/// 避免各处再靠 Method/字符串拼接去猜旧入口。
/// </summary>
public sealed class MotorYLegacyAlgorithmRoute
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int MethodValue { get; init; }
    public string MethodKey { get; init; } = string.Empty;
    public string ProfileKey { get; init; } = string.Empty;
    public string LegacyEnumName { get; init; } = string.Empty;
    public string LegacyFormName { get; init; } = string.Empty;
    public string LegacyAlgorithmEntry { get; init; } = string.Empty;
    public string LegacyMethodName { get; init; } = string.Empty;
    public string LegacySettingsMethodName { get; init; } = string.Empty;
    public bool IsBaselineMethod { get; init; }
}

public static class MotorYLegacyAlgorithmRouteResolver
{
    public static MotorYLegacyAlgorithmRoute? Resolve(string? canonicalCode, int? methodValue)
    {
        var profile = MotorYMethodProfileCatalog.TryGet(canonicalCode, methodValue);
        if (profile is null || string.IsNullOrWhiteSpace(canonicalCode) || !methodValue.HasValue)
        {
            return null;
        }

        return new MotorYLegacyAlgorithmRoute
        {
            CanonicalCode = canonicalCode,
            MethodValue = methodValue.Value,
            MethodKey = $"{canonicalCode}:{methodValue.Value}",
            ProfileKey = profile.ProfileKey,
            LegacyEnumName = profile.LegacyEnumName,
            LegacyFormName = profile.LegacyFormName,
            LegacyAlgorithmEntry = profile.LegacyAlgorithmEntry,
            LegacyMethodName = profile.LegacyMethodName,
            LegacySettingsMethodName = profile.LegacySettingsMethodName,
            IsBaselineMethod = profile.IsBaselineEnumValue
        };
    }
}
