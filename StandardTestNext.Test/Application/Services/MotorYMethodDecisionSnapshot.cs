namespace StandardTestNext.Test.Application.Services;

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
}
