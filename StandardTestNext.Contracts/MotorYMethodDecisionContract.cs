namespace StandardTestNext.Contracts;

public sealed class MotorYMethodDistributionContract
{
    public int MethodValue { get; init; }
    public int Count { get; init; }
    public double Share { get; init; }
    public MotorYBuildProfileContract? Profile { get; init; }
}

public sealed class MotorYMethodDecisionContract
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public MotorYBuildProfileContract? BaselineProfile { get; init; }
    public int BaselineCount { get; init; }
    public MotorYBuildProfileContract? DominantProfile { get; init; }
    public int DominantCount { get; init; }
    public MotorYBuildProfileContract? RecommendedProfile { get; init; }
    public string RecommendedStrategy { get; init; } = string.Empty;
    public bool ShouldPrioritizeDominantOverBaseline { get; init; }
    public double DominantShare { get; init; }
    public double DominantOverrideThreshold { get; init; }
    public IReadOnlyList<MotorYMethodDistributionContract> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionContract>();
}
