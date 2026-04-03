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
    public MotorYLegacyAlgorithmRoute? RecommendedRoute { get; init; }
    public string RecommendedStrategy { get; init; } = string.Empty;
    public bool ShouldPrioritizeDominantOverBaseline { get; init; }
    public double DominantShare { get; init; }
    public double BaselineShare { get; init; }
    public double DominantOverrideThreshold { get; init; }
    public int DominantLeadCount { get; init; }
    public int DominantLeadPercentagePoints { get; init; }
    public string RecommendationReason { get; init; } = string.Empty;
    public string RecommendedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
