namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYMethodDistributionSnapshot
{
    public int MethodValue { get; init; }
    public int Count { get; init; }
    public double Share { get; init; }
    public MotorYLegacyAlgorithmRoute? Route { get; init; }
}

public sealed class MotorYMethodDecisionSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public MotorYLegacyAlgorithmRoute? BaselineRoute { get; init; }
    public int BaselineCount { get; init; }
    public MotorYLegacyAlgorithmRoute? DominantRoute { get; init; }
    public int DominantCount { get; init; }
    public bool ShouldPrioritizeDominantOverBaseline { get; init; }
    public double DominantShare { get; init; }
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
