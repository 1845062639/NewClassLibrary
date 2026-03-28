namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYLegacyCodeSelectionSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string RecommendedLegacyCode { get; init; } = string.Empty;
    public string DominantLegacyCode { get; init; } = string.Empty;
    public int RecommendedLegacyCodeCount { get; init; }
    public double RecommendedLegacyCodeShare { get; init; }
    public IReadOnlyList<MotorYLegacyCodeDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYLegacyCodeDistributionSnapshot>();
    public string Summary { get; init; } = string.Empty;
}
