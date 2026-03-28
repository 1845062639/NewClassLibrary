namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYMethodAdaptationPlanSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public MotorYLegacyAlgorithmRoute? BaselineRoute { get; init; }
    public int BaselineCount { get; init; }
    public double BaselineShare { get; init; }
    public MotorYLegacyAlgorithmRoute? DominantRoute { get; init; }
    public int DominantCount { get; init; }
    public double DominantShare { get; init; }
    public MotorYLegacyAlgorithmRoute? SelectedRoute { get; init; }
    public int SelectedCount { get; init; }
    public double SelectedShare { get; init; }
    public string SelectionStrategy { get; init; } = string.Empty;
    public bool ShouldUseDominantRoute { get; init; }
    public double DominantOverrideThreshold { get; init; }
    public int DominantLeadCount { get; init; }
    public int DominantLeadPercentagePoints { get; init; }
    public double SelectedLeadCountVsBaseline { get; init; }
    public int SelectedLeadPercentagePointsVsBaseline { get; init; }
    public string SelectionReason { get; init; } = string.Empty;
    public string AlgorithmEntry { get; init; } = string.Empty;
    public string SettingsMethodName { get; init; } = string.Empty;
    public string LegacyMethodName { get; init; } = string.Empty;
    public bool RequiresRatedParams { get; init; }
    public IReadOnlyList<string> UpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public int CoveredRequiredPayloadFieldCount { get; init; }
    public int MissingRequiredPayloadFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredPayloadFields { get; init; } = Array.Empty<string>();
    public string RequiredPayloadFieldCoverageSummary { get; init; } = string.Empty;
    public string DependencyNotes { get; init; } = string.Empty;
    public string SelectedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
