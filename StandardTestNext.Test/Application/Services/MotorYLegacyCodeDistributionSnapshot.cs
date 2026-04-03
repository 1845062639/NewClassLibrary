namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYLegacyCodeDistributionSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string LegacyCode { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Share { get; init; }
}
