namespace StandardTestNext.Contracts;

public sealed class MotorYMethodDecisionContract
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public MotorYBuildProfileContract? BaselineProfile { get; init; }
    public int BaselineCount { get; init; }
    public MotorYBuildProfileContract? DominantProfile { get; init; }
    public int DominantCount { get; init; }
    public bool ShouldPrioritizeDominantOverBaseline { get; init; }
    public double DominantShare { get; init; }
}
