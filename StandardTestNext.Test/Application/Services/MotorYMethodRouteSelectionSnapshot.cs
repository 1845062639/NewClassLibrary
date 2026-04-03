namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 供 Motor_Y 算法适配层直接消费的“本次应走哪条旧方法路由”摘要，
/// 把 baseline / dominant / selected 的差异与理由收敛成统一对象，
/// 避免后续 adapter / report / app 重复手算。
/// </summary>
public sealed class MotorYMethodRouteSelectionSnapshot
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
    public int SelectedLeadCountVsBaseline { get; init; }
    public int SelectedLeadPercentagePointsVsBaseline { get; init; }
    public string SelectionReason { get; init; } = string.Empty;
    public string SelectedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
