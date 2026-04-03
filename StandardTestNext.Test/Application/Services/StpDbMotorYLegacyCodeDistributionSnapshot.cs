namespace StandardTestNext.Test.Application.Services;

public sealed class StpDbMotorYLegacyCodeDistributionSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string LegacyCode { get; init; } = string.Empty;
    public int? Method { get; init; }
    public int Count { get; init; }
    public string MethodKey { get; init; } = string.Empty;
    public MotorYLegacyAlgorithmRoute? Route { get; init; }
}
