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

public sealed class MotorYDecisionAnchorObservationRuleSnapshot
{
    public string AnchorKey { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public bool CoveredByObservedPayload { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public sealed class MotorYDecisionAnchorResolutionSnapshot
{
    public string AnchorKey { get; init; } = string.Empty;
    public bool ResolvedByObservedPayload { get; init; }
    public bool PartiallyResolvedByObservedPayload { get; init; }
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public string ResolutionStage { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed class MotorYLegacyUpstreamCodeDistributionSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string LegacyCode { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Share { get; init; }
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
    public IReadOnlyDictionary<string, IReadOnlyList<string>> UpstreamLegacyAliases { get; init; } = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
    public IReadOnlyList<MotorYLegacyUpstreamCodeDistributionSnapshot> UpstreamLegacyCodeDistributions { get; init; } = Array.Empty<MotorYLegacyUpstreamCodeDistributionSnapshot>();
    public int ObservedUpstreamCanonicalCodeCount { get; init; }
    public IReadOnlyList<string> ObservedUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ObservedUpstreamLegacyCodes { get; init; } = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public bool UpstreamDependenciesSatisfied { get; init; }
    public string UpstreamDependencySummary { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredIntermediateResultFields { get; init; } = Array.Empty<string>();
    public int CoveredRequiredIntermediateResultFieldCount { get; init; }
    public int MissingRequiredIntermediateResultFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredIntermediateResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredIntermediateResultFields { get; init; } = Array.Empty<string>();
    public double RequiredIntermediateResultFieldCoverageRatio { get; init; }
    public int RequiredIntermediateResultFieldCoveragePercentagePoints { get; init; }
    public string RequiredIntermediateResultFieldCoverageSummary { get; init; } = string.Empty;
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
    public IReadOnlyList<string> ObservedAlgorithmInputFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingAlgorithmInputFields { get; init; } = Array.Empty<string>();
    public int ObservedAlgorithmInputFieldCount { get; init; }
    public int MissingAlgorithmInputFieldCount { get; init; }
    public double AlgorithmInputFieldCoverageRatio { get; init; }
    public int AlgorithmInputFieldCoveragePercentagePoints { get; init; }
    public string AlgorithmInputFieldCoverageSummary { get; init; } = string.Empty;
    public bool RawDataSignalsReady { get; init; }
    public IReadOnlyList<string> RequiredStructuredPayloadSignals { get; init; } = Array.Empty<string>();
    public int MinimumRawSampleCount { get; init; }
    public bool RawSampleCountReady { get; init; }
    public string RawSampleCountReadinessSummary { get; init; } = string.Empty;
    public int MinimumStructuredPayloadSampleCount { get; init; }
    public bool StructuredPayloadSampleCountReady { get; init; }
    public string StructuredPayloadSampleCountReadinessSummary { get; init; } = string.Empty;
    public int MinimumStructuredResultSampleCount { get; init; }
    public bool StructuredResultSampleCountReady { get; init; }
    public string StructuredResultSampleCountReadinessSummary { get; init; } = string.Empty;
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
    public IReadOnlyList<string> LegacyDecisionAnchors { get; init; } = Array.Empty<string>();
    public int CoveredLegacyDecisionAnchorCount { get; init; }
    public int MissingLegacyDecisionAnchorCount { get; init; }
    public IReadOnlyList<string> CoveredLegacyDecisionAnchors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingLegacyDecisionAnchors { get; init; } = Array.Empty<string>();
    public double LegacyDecisionAnchorCoverageRatio { get; init; }
    public int LegacyDecisionAnchorCoveragePercentagePoints { get; init; }
    public bool LegacyDecisionAnchorsBackedByObservedPayload { get; init; }
    public IReadOnlyList<string> LegacyDecisionAnchorsObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<MotorYObservedAlgorithmEvidenceGapSnapshot> LegacyDecisionAnchorsObservedPayloadGaps { get; init; } = Array.Empty<MotorYObservedAlgorithmEvidenceGapSnapshot>();
    public IReadOnlyList<MotorYDecisionAnchorObservationRuleSnapshot> LegacyDecisionAnchorObservationRules { get; init; } = Array.Empty<MotorYDecisionAnchorObservationRuleSnapshot>();
    public IReadOnlyList<MotorYDecisionAnchorResolutionSnapshot> LegacyDecisionAnchorResolutions { get; init; } = Array.Empty<MotorYDecisionAnchorResolutionSnapshot>();
    public int CoveredLegacyDecisionAnchorObservationRuleCount { get; init; }
    public int MissingLegacyDecisionAnchorObservationRuleCount { get; init; }
    public int ResolvedLegacyDecisionAnchorCount { get; init; }
    public int PartialLegacyDecisionAnchorCount { get; init; }
    public int MissingLegacyDecisionAnchorResolutionCount { get; init; }
    public double LegacyDecisionAnchorObservationRuleCoverageRatio { get; init; }
    public int LegacyDecisionAnchorObservationRuleCoveragePercentagePoints { get; init; }
    public double LegacyDecisionAnchorResolutionCoverageRatio { get; init; }
    public int LegacyDecisionAnchorResolutionCoveragePercentagePoints { get; init; }
    public string LegacyDecisionAnchorObservationRuleSummary { get; init; } = string.Empty;
    public string LegacyDecisionAnchorResolutionSummary { get; init; } = string.Empty;
    public string LegacyDecisionAnchorsObservedPayloadSummary { get; init; } = string.Empty;
    public string FormulaSignalSummary { get; init; } = string.Empty;
    public string LegacyAlgorithmRuleSummary { get; init; } = string.Empty;
    public string LegacyDecisionAnchorSummary { get; init; } = string.Empty;
    public string SelectedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
    public IReadOnlyList<MotorYDependencyBucketSummarySnapshot> DependencyBuckets { get; init; } = Array.Empty<MotorYDependencyBucketSummarySnapshot>();
    public IReadOnlyList<MotorYMethodDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYMethodDistributionSnapshot>();
}
