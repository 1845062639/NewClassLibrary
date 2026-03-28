namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYMethodAdaptationPlanSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public MotorYLegacyAlgorithmRoute? BaselineRoute { get; init; }
    public int BaselineCount { get; init; }
    public MotorYLegacyAlgorithmRoute? DominantRoute { get; init; }
    public int DominantCount { get; init; }
    public MotorYLegacyAlgorithmRoute? SelectedRoute { get; init; }
    public int SelectedCount { get; init; }
    public string SelectionStrategy { get; init; } = string.Empty;
    public bool ShouldUseDominantRoute { get; init; }
    public double DominantShare { get; init; }
    public double DominantOverrideThreshold { get; init; }
    public string AlgorithmEntry { get; init; } = string.Empty;
    public string SettingsMethodName { get; init; } = string.Empty;
    public string LegacyMethodName { get; init; } = string.Empty;
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
