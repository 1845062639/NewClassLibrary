namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYObservedAlgorithmEvidenceGapSnapshot
{
    public string SignalOrRule { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public bool CoveredByObservedPayload { get; init; }
    public string Summary { get; init; } = string.Empty;
}

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
    public string RecommendedLegacyCode { get; init; } = string.Empty;
    public string DominantLegacyCode { get; init; } = string.Empty;
    public int RecommendedLegacyCodeCount { get; init; }
    public double RecommendedLegacyCodeShare { get; init; }
    public string LegacyCodeSelectionSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYLegacyCodeDistributionSnapshot> LegacyCodeDistributions { get; init; } = Array.Empty<MotorYLegacyCodeDistributionSnapshot>();
    public bool RequiresRatedParams { get; init; }
    public IReadOnlyList<string> UpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public int ObservedUpstreamCanonicalCodeCount { get; init; }
    public IReadOnlyList<string> ObservedUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public bool UpstreamDependenciesSatisfied { get; init; }
    public string UpstreamDependencySummary { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredResultFields { get; init; } = Array.Empty<string>();
    public int CoveredRequiredResultFieldCount { get; init; }
    public int MissingRequiredResultFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredResultFields { get; init; } = Array.Empty<string>();
    public double RequiredResultFieldCoverageRatio { get; init; }
    public int RequiredResultFieldCoveragePercentagePoints { get; init; }
    public string RequiredResultFieldCoverageSummary { get; init; } = string.Empty;
    public int CoveredRequiredPayloadFieldCount { get; init; }
    public int MissingRequiredPayloadFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredPayloadFields { get; init; } = Array.Empty<string>();
    public double RequiredPayloadFieldCoverageRatio { get; init; }
    public int RequiredPayloadFieldCoveragePercentagePoints { get; init; }
    public bool SamplePayloadAvailable { get; init; }
    public string RequiredPayloadFieldCoverageSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredRawDataSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedRawDataSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingRawDataSignals { get; init; } = Array.Empty<string>();
    public int RawDataSignalCoveredCount { get; init; }
    public int RawDataSignalMissingCount { get; init; }
    public int RawDataSampleCount { get; init; }
    public bool RawDataListAvailable { get; init; }
    public double RawDataSignalCoverageRatio { get; init; }
    public int RawDataSignalCoveragePercentagePoints { get; init; }
    public string RawDataSignalCoverageSummary { get; init; } = string.Empty;
    public int CoveredRequiredRatedParamFieldCount { get; init; }
    public int MissingRequiredRatedParamFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public double RequiredRatedParamFieldCoverageRatio { get; init; }
    public int RequiredRatedParamFieldCoveragePercentagePoints { get; init; }
    public bool RatedParamsAvailable { get; init; }
    public string RequiredRatedParamFieldCoverageSummary { get; init; } = string.Empty;
    public bool LegacyAlgorithmInputsReady { get; init; }
    public bool RawDataSignalsReady { get; init; }
    public IReadOnlyList<string> RequiredStructuredPayloadSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedStructuredPayloadSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingStructuredPayloadSignals { get; init; } = Array.Empty<string>();
    public int StructuredPayloadSignalCoveredCount { get; init; }
    public int StructuredPayloadSignalMissingCount { get; init; }
    public int StructuredPayloadSampleCount { get; init; }
    public bool StructuredPayloadAvailable { get; init; }
    public double StructuredPayloadSignalCoverageRatio { get; init; }
    public int StructuredPayloadSignalCoveragePercentagePoints { get; init; }
    public string StructuredPayloadSignalCoverageSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredStructuredResultSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedStructuredResultSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingStructuredResultSignals { get; init; } = Array.Empty<string>();
    public int StructuredResultSignalCoveredCount { get; init; }
    public int StructuredResultSignalMissingCount { get; init; }
    public int StructuredResultSampleCount { get; init; }
    public bool StructuredResultAvailable { get; init; }
    public double StructuredResultSignalCoverageRatio { get; init; }
    public int StructuredResultSignalCoveragePercentagePoints { get; init; }
    public string StructuredResultSignalCoverageSummary { get; init; } = string.Empty;
    public string LegacyAlgorithmInputReadinessSummary { get; init; } = string.Empty;
    public string DependencyNotes { get; init; } = string.Empty;
    public IReadOnlyList<string> FormulaSignals { get; init; } = Array.Empty<string>();
    public int CoveredFormulaSignalCount { get; init; }
    public int MissingFormulaSignalCount { get; init; }
    public IReadOnlyList<string> CoveredFormulaSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingFormulaSignals { get; init; } = Array.Empty<string>();
    public double FormulaSignalCoverageRatio { get; init; }
    public int FormulaSignalCoveragePercentagePoints { get; init; }
    public bool FormulaSignalsBackedByObservedPayload { get; init; }
    public IReadOnlyList<string> FormulaSignalsObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<MotorYObservedAlgorithmEvidenceGapSnapshot> FormulaSignalObservedPayloadGaps { get; init; } = Array.Empty<MotorYObservedAlgorithmEvidenceGapSnapshot>();
    public string FormulaSignalsObservedPayloadSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> LegacyAlgorithmRules { get; init; } = Array.Empty<string>();
    public int CoveredLegacyAlgorithmRuleCount { get; init; }
    public int MissingLegacyAlgorithmRuleCount { get; init; }
    public IReadOnlyList<string> CoveredLegacyAlgorithmRules { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingLegacyAlgorithmRules { get; init; } = Array.Empty<string>();
    public double LegacyAlgorithmRuleCoverageRatio { get; init; }
    public int LegacyAlgorithmRuleCoveragePercentagePoints { get; init; }
    public bool LegacyAlgorithmRulesBackedByObservedPayload { get; init; }
    public IReadOnlyList<string> LegacyAlgorithmRulesObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<MotorYObservedAlgorithmEvidenceGapSnapshot> LegacyAlgorithmRulesObservedPayloadGaps { get; init; } = Array.Empty<MotorYObservedAlgorithmEvidenceGapSnapshot>();
    public string LegacyAlgorithmRulesObservedPayloadSummary { get; init; } = string.Empty;
    public string FormulaSignalSummary { get; init; } = string.Empty;
    public string LegacyAlgorithmRuleSummary { get; init; } = string.Empty;
    public string SelectedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
