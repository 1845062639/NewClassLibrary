namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYDependencyBucketSummarySnapshot
{
    public string BucketKey { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int RequiredCount { get; init; }
    public int CoveredCount { get; init; }
    public int MissingCount { get; init; }
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public IReadOnlyList<string> RequiredItems { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredItems { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingItems { get; init; } = Array.Empty<string>();
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYDependencyBucketSummaryFactory
{
    public static IReadOnlyList<MotorYDependencyBucketSummarySnapshot> Create(
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        MotorYStructuredListCoverageSnapshot formulaCoverage,
        MotorYStructuredListCoverageSnapshot ruleCoverage,
        MotorYStructuredListCoverageSnapshot decisionAnchorCoverage)
    {
        return new MotorYDependencyBucketSummarySnapshot[]
        {
            new()
            {
                BucketKey = "upstream",
                DisplayName = "上游试验依赖",
                RequiredCount = upstream.ObservedUpstreamCanonicalCodeCount + upstream.MissingUpstreamCanonicalCodes.Count,
                CoveredCount = upstream.ObservedUpstreamCanonicalCodeCount,
                MissingCount = upstream.MissingUpstreamCanonicalCodes.Count,
                CoverageRatio = CalculateRatio(upstream.ObservedUpstreamCanonicalCodeCount, upstream.ObservedUpstreamCanonicalCodeCount + upstream.MissingUpstreamCanonicalCodes.Count),
                CoveragePercentagePoints = CalculatePercentagePoints(upstream.ObservedUpstreamCanonicalCodeCount, upstream.ObservedUpstreamCanonicalCodeCount + upstream.MissingUpstreamCanonicalCodes.Count),
                RequiredItems = upstream.ObservedUpstreamCanonicalCodes.Concat(upstream.MissingUpstreamCanonicalCodes).ToArray(),
                CoveredItems = upstream.ObservedUpstreamCanonicalCodes,
                MissingItems = upstream.MissingUpstreamCanonicalCodes,
                Summary = upstream.UpstreamDependencySummary
            },
            new()
            {
                BucketKey = "payload-fields",
                DisplayName = "Payload字段",
                RequiredCount = payloadCoverage.CoveredRequiredPayloadFieldCount + payloadCoverage.MissingRequiredPayloadFieldCount,
                CoveredCount = payloadCoverage.CoveredRequiredPayloadFieldCount,
                MissingCount = payloadCoverage.MissingRequiredPayloadFieldCount,
                CoverageRatio = payloadCoverage.RequiredPayloadFieldCoverageRatio,
                CoveragePercentagePoints = payloadCoverage.RequiredPayloadFieldCoveragePercentagePoints,
                RequiredItems = payloadCoverage.CoveredRequiredPayloadFields.Concat(payloadCoverage.MissingRequiredPayloadFields).ToArray(),
                CoveredItems = payloadCoverage.CoveredRequiredPayloadFields,
                MissingItems = payloadCoverage.MissingRequiredPayloadFields,
                Summary = payloadCoverage.RequiredPayloadFieldCoverageSummary
            },
            new()
            {
                BucketKey = "rated-params",
                DisplayName = "额定参数字段",
                RequiredCount = ratedCoverage.CoveredRequiredRatedParamFieldCount + ratedCoverage.MissingRequiredRatedParamFieldCount,
                CoveredCount = ratedCoverage.CoveredRequiredRatedParamFieldCount,
                MissingCount = ratedCoverage.MissingRequiredRatedParamFieldCount,
                CoverageRatio = ratedCoverage.RequiredRatedParamFieldCoverageRatio,
                CoveragePercentagePoints = ratedCoverage.RequiredRatedParamFieldCoveragePercentagePoints,
                RequiredItems = ratedCoverage.CoveredRequiredRatedParamFields.Concat(ratedCoverage.MissingRequiredRatedParamFields).ToArray(),
                CoveredItems = ratedCoverage.CoveredRequiredRatedParamFields,
                MissingItems = ratedCoverage.MissingRequiredRatedParamFields,
                Summary = ratedCoverage.RequiredRatedParamFieldCoverageSummary
            },
            new()
            {
                BucketKey = "result-fields",
                DisplayName = "结果字段",
                RequiredCount = resultCoverage.CoveredRequiredResultFieldCount + resultCoverage.MissingRequiredResultFieldCount,
                CoveredCount = resultCoverage.CoveredRequiredResultFieldCount,
                MissingCount = resultCoverage.MissingRequiredResultFieldCount,
                CoverageRatio = resultCoverage.RequiredResultFieldCoverageRatio,
                CoveragePercentagePoints = resultCoverage.RequiredResultFieldCoveragePercentagePoints,
                RequiredItems = resultCoverage.CoveredRequiredResultFields.Concat(resultCoverage.MissingRequiredResultFields).ToArray(),
                CoveredItems = resultCoverage.CoveredRequiredResultFields,
                MissingItems = resultCoverage.MissingRequiredResultFields,
                Summary = resultCoverage.RequiredResultFieldCoverageSummary
            },
            new()
            {
                BucketKey = "raw-data-signals",
                DisplayName = "RawData信号",
                RequiredCount = rawDataCoverage.RequiredSignals.Count,
                CoveredCount = rawDataCoverage.ObservedSignals.Count,
                MissingCount = rawDataCoverage.MissingSignals.Count,
                CoverageRatio = rawDataCoverage.CoverageRatio,
                CoveragePercentagePoints = rawDataCoverage.CoveragePercentagePoints,
                RequiredItems = rawDataCoverage.RequiredSignals,
                CoveredItems = rawDataCoverage.ObservedSignals,
                MissingItems = rawDataCoverage.MissingSignals,
                Summary = rawDataCoverage.Summary
            },
            new()
            {
                BucketKey = "structured-payload-signals",
                DisplayName = "结构化Payload信号",
                RequiredCount = structuredPayloadCoverage.CoveredSignalCount + structuredPayloadCoverage.MissingSignalCount,
                CoveredCount = structuredPayloadCoverage.CoveredSignalCount,
                MissingCount = structuredPayloadCoverage.MissingSignalCount,
                CoverageRatio = structuredPayloadCoverage.CoverageRatio,
                CoveragePercentagePoints = structuredPayloadCoverage.CoveragePercentagePoints,
                RequiredItems = structuredPayloadCoverage.ObservedSignals.Concat(structuredPayloadCoverage.MissingSignals).ToArray(),
                CoveredItems = structuredPayloadCoverage.ObservedSignals,
                MissingItems = structuredPayloadCoverage.MissingSignals,
                Summary = structuredPayloadCoverage.Summary
            },
            new()
            {
                BucketKey = "structured-result-signals",
                DisplayName = "结构化结果信号",
                RequiredCount = structuredResultCoverage.CoveredSignalCount + structuredResultCoverage.MissingSignalCount,
                CoveredCount = structuredResultCoverage.CoveredSignalCount,
                MissingCount = structuredResultCoverage.MissingSignalCount,
                CoverageRatio = structuredResultCoverage.CoverageRatio,
                CoveragePercentagePoints = structuredResultCoverage.CoveragePercentagePoints,
                RequiredItems = structuredResultCoverage.ObservedSignals.Concat(structuredResultCoverage.MissingSignals).ToArray(),
                CoveredItems = structuredResultCoverage.ObservedSignals,
                MissingItems = structuredResultCoverage.MissingSignals,
                Summary = structuredResultCoverage.Summary
            },
            new()
            {
                BucketKey = "formula-signals",
                DisplayName = "公式语义信号",
                RequiredCount = formulaCoverage.RequiredCount,
                CoveredCount = formulaCoverage.CoveredCount,
                MissingCount = formulaCoverage.MissingCount,
                CoverageRatio = formulaCoverage.CoverageRatio,
                CoveragePercentagePoints = formulaCoverage.CoveragePercentagePoints,
                RequiredItems = formulaCoverage.CoveredItems.Concat(formulaCoverage.MissingItems).ToArray(),
                CoveredItems = formulaCoverage.CoveredItems,
                MissingItems = formulaCoverage.MissingItems,
                Summary = formulaCoverage.Summary
            },
            new()
            {
                BucketKey = "legacy-rules",
                DisplayName = "旧算法规则语义",
                RequiredCount = ruleCoverage.RequiredCount,
                CoveredCount = ruleCoverage.CoveredCount,
                MissingCount = ruleCoverage.MissingCount,
                CoverageRatio = ruleCoverage.CoverageRatio,
                CoveragePercentagePoints = ruleCoverage.CoveragePercentagePoints,
                RequiredItems = ruleCoverage.CoveredItems.Concat(ruleCoverage.MissingItems).ToArray(),
                CoveredItems = ruleCoverage.CoveredItems,
                MissingItems = ruleCoverage.MissingItems,
                Summary = ruleCoverage.Summary
            },
            new()
            {
                BucketKey = "legacy-decision-anchors",
                DisplayName = "旧算法决策锚点",
                RequiredCount = decisionAnchorCoverage.RequiredCount,
                CoveredCount = decisionAnchorCoverage.CoveredCount,
                MissingCount = decisionAnchorCoverage.MissingCount,
                CoverageRatio = decisionAnchorCoverage.CoverageRatio,
                CoveragePercentagePoints = decisionAnchorCoverage.CoveragePercentagePoints,
                RequiredItems = decisionAnchorCoverage.CoveredItems.Concat(decisionAnchorCoverage.MissingItems).ToArray(),
                CoveredItems = decisionAnchorCoverage.CoveredItems,
                MissingItems = decisionAnchorCoverage.MissingItems,
                Summary = decisionAnchorCoverage.Summary
            }
        };
    }

    private static double CalculateRatio(int covered, int total)
    {
        return total <= 0
            ? 1d
            : Math.Round((double)covered / total, 4, MidpointRounding.AwayFromZero);
    }

    private static int CalculatePercentagePoints(int covered, int total)
    {
        return total <= 0
            ? 100
            : (int)Math.Round((double)covered / total * 100d, MidpointRounding.AwayFromZero);
    }
}
