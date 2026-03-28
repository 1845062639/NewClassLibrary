namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 在 next-gen 中沉淀旧 Motor_Y Method 的“业务变体 / 算法家族”标签，
/// 让后续 builder / adapter / report 可以直接基于 stp.db 真实口径路由，
/// 避免继续散落地判断 delivery / companion / baseline 等分支。
/// </summary>
public static class MotorYLegacyVariantKinds
{
    public const string Baseline = "baseline";
    public const string Delivery = "delivery";
    public const string Companion = "companion";
    public const string DeliveryCompanion = "delivery-companion";
    public const string LegacyAlias = "legacy-alias";
    public const string OtherVariant = "other-variant";
}

public static class MotorYLegacyAlgorithmFamilies
{
    public const string DirectCurrentResistance = "DirectCurrentResistance";
    public const string NoLoad = "NoLoad";
    public const string Thermal = "Thermal";
    public const string LoadA = "LoadA";
    public const string LoadB = "LoadB";
    public const string LockedRotor = "LockedRotor";
}
