using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYMethodAdaptationPlanContractMapper
{
    public static MotorYMethodAdaptationPlanContract Map(
        MotorYMethodDecisionSnapshot snapshot,
        Func<MotorYLegacyAlgorithmRoute?, MotorYBuildProfileContract?> profileMapper,
        IReadOnlyList<MotorYLegacyCodeDistributionSnapshot>? legacyCodeDistributions = null)
    {
        var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(snapshot);
        var selectedProfile = selection.SelectedRoute;
        var dependencyProfile = MotorYLegacyAlgorithmDependencyCatalog.TryGet(selection.CanonicalCode);
        var legacyCodeSelection = BuildLegacyCodeSelection(selection.CanonicalCode, legacyCodeDistributions);
        var requiredPayloadFields = dependencyProfile?.RequiredPayloadFields ?? Array.Empty<string>();
        var upstream = MotorYUpstreamDependencySnapshotFactory.Create(
            selection.CanonicalCode,
            dependencyProfile?.UpstreamCanonicalCodes ?? Array.Empty<string>(),
            Array.Empty<string>(),
            null);
        var upstreamLegacyAliases = dependencyProfile?.UpstreamLegacyAliases
            ?? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        var upstreamLegacyCodeDistributions = upstreamLegacyAliases
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .SelectMany(pair => MotorYLegacyUpstreamCodeCatalog.BuildDistributions(pair.Key, Array.Empty<string>()))
            .ToArray();
        var coverage = MotorYRequiredPayloadFieldCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            requiredPayloadFields,
            null);
        var ratedCoverage = MotorYRequiredRatedParamFieldCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
            null);
        var rawDataSignalCoverage = MotorYRawDataSignalCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            null);
        var resultCoverage = MotorYRequiredResultFieldCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            dependencyProfile?.RequiredResultFields ?? Array.Empty<string>(),
            null);
        var intermediateResultCoverage = MotorYRequiredResultFieldCoverageEvaluator.Evaluate(
            selection.CanonicalCode,
            dependencyProfile?.RequiredIntermediateResultFields ?? Array.Empty<string>(),
            null);
        var structuredPayloadCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
            dependencyProfile?.RequiredStructuredPayloadSignals,
            null,
            "structured payload signals");
        var structuredResultCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
            dependencyProfile?.RequiredStructuredResultSignals,
            null,
            "structured result signals");
        var observedStructuredSignals = structuredPayloadCoverage.ObservedSignals
            .Concat(structuredResultCoverage.ObservedSignals)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var formulaEvidenceObservedFields = resultCoverage.CoveredRequiredResultFields
            .Concat(rawDataSignalCoverage.ObservedSignals)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var formulaEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildFormulaSignalEvidence(
            selection.CanonicalCode,
            formulaEvidenceObservedFields,
            observedStructuredSignals);
        var formulaCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
            dependencyProfile?.FormulaSignals,
            formulaEvidence.ObservedPayloadFields,
            "formula signals");
        var ruleObservedFields = coverage.CoveredRequiredPayloadFields
            .Concat(ratedCoverage.CoveredRequiredRatedParamFields)
            .Concat(resultCoverage.CoveredRequiredResultFields)
            .Concat(rawDataSignalCoverage.ObservedSignals)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var ruleEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildLegacyRuleEvidence(
            selection.CanonicalCode,
            ruleObservedFields,
            observedStructuredSignals);
        var ruleCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
            dependencyProfile?.LegacyAlgorithmRules,
            ruleEvidence.ObservedPayloadFields,
            "legacy algorithm rules");
        var decisionAnchorObservedFields = ruleObservedFields;
        var decisionAnchorEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildLegacyDecisionAnchorEvidence(
            selection.CanonicalCode,
            decisionAnchorObservedFields,
            observedStructuredSignals);
        var decisionAnchorCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
            dependencyProfile?.LegacyDecisionAnchors,
            decisionAnchorEvidence.ObservedPayloadFields,
            "legacy decision anchors");
        var observedAlgorithmInputFields = coverage.CoveredRequiredPayloadFields
            .Concat(ratedCoverage.CoveredRequiredRatedParamFields)
            .Concat(resultCoverage.CoveredRequiredResultFields)
            .Concat(intermediateResultCoverage.CoveredRequiredResultFields)
            .Concat(rawDataSignalCoverage.ObservedSignals)
            .Concat(structuredPayloadCoverage.ObservedSignals)
            .Concat(structuredResultCoverage.ObservedSignals)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var missingAlgorithmInputFields = upstream.MissingUpstreamCanonicalCodes
            .Concat(coverage.MissingRequiredPayloadFields)
            .Concat(ratedCoverage.MissingRequiredRatedParamFields)
            .Concat(resultCoverage.MissingRequiredResultFields)
            .Concat(intermediateResultCoverage.MissingRequiredResultFields)
            .Concat(rawDataSignalCoverage.MissingSignals)
            .Concat(structuredPayloadCoverage.MissingSignals)
            .Concat(structuredResultCoverage.MissingSignals)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var totalAlgorithmInputFieldCount = observedAlgorithmInputFields.Length + missingAlgorithmInputFields.Length;
        var algorithmInputFieldCoverageRatio = totalAlgorithmInputFieldCount == 0
            ? 1d
            : Math.Round((double)observedAlgorithmInputFields.Length / totalAlgorithmInputFieldCount, 4, MidpointRounding.AwayFromZero);
        var algorithmInputFieldCoveragePercentagePoints = (int)Math.Round(algorithmInputFieldCoverageRatio * 100d, MidpointRounding.AwayFromZero);
        var algorithmInputFieldCoverageSummary = $"algorithm input fields covered {observedAlgorithmInputFields.Length}/{totalAlgorithmInputFieldCount} ({algorithmInputFieldCoveragePercentagePoints}pp); missing: {(missingAlgorithmInputFields.Length == 0 ? "none" : string.Join(", ", missingAlgorithmInputFields))}; observed: {(observedAlgorithmInputFields.Length == 0 ? "none" : string.Join(", ", observedAlgorithmInputFields))}";
        var rawDataSignalsReady = rawDataSignalCoverage.MissingSignals.Count == 0;
        var structuredSignalsReady = structuredPayloadCoverage.MissingSignalCount == 0
            && structuredResultCoverage.MissingSignalCount == 0;
        var requiredResultFieldsReady = resultCoverage.MissingRequiredResultFieldCount == 0;
        var requiredIntermediateResultFieldsReady = intermediateResultCoverage.MissingRequiredResultFieldCount == 0;
        var legacyAlgorithmInputsReady = upstream.UpstreamDependenciesSatisfied
            && coverage.MissingRequiredPayloadFieldCount == 0
            && ratedCoverage.MissingRequiredRatedParamFieldCount == 0
            && requiredResultFieldsReady
            && requiredIntermediateResultFieldsReady
            && rawDataSignalsReady
            && structuredSignalsReady;
        var legacyAlgorithmInputReadinessSummary = BuildLegacyAlgorithmInputReadinessSummary(
            upstream,
            coverage,
            ratedCoverage,
            resultCoverage,
            intermediateResultCoverage,
            rawDataSignalCoverage,
            structuredPayloadCoverage,
            structuredResultCoverage,
            legacyAlgorithmInputsReady);
        var dependencyBuckets = MotorYDependencyBucketSummaryFactory.Create(
            upstream,
            coverage,
            ratedCoverage,
            resultCoverage,
            intermediateResultCoverage,
            rawDataSignalCoverage,
            structuredPayloadCoverage,
            structuredResultCoverage,
            formulaCoverage,
            ruleCoverage,
            decisionAnchorCoverage);

        return new MotorYMethodAdaptationPlanContract
        {
            CanonicalCode = selection.CanonicalCode,
            TotalCount = selection.TotalCount,
            BaselineProfile = profileMapper(selection.BaselineRoute),
            BaselineCount = selection.BaselineCount,
            BaselineShare = selection.BaselineShare,
            DominantProfile = profileMapper(selection.DominantRoute),
            DominantCount = selection.DominantCount,
            DominantShare = selection.DominantShare,
            SelectedProfile = profileMapper(selectedProfile),
            SelectedCount = selection.SelectedCount,
            SelectedShare = selection.SelectedShare,
            SelectionStrategy = selection.SelectionStrategy,
            ShouldUseDominantRoute = selection.ShouldUseDominantRoute,
            DominantOverrideThreshold = selection.DominantOverrideThreshold,
            DominantLeadCount = selection.DominantLeadCount,
            DominantLeadPercentagePoints = selection.DominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = selection.SelectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = selection.SelectedLeadPercentagePointsVsBaseline,
            SelectionReason = selection.SelectionReason,
            AlgorithmEntry = selectedProfile?.LegacyAlgorithmEntry ?? string.Empty,
            SettingsMethodName = selectedProfile?.LegacySettingsMethodName ?? string.Empty,
            LegacyMethodName = selectedProfile?.LegacyMethodName ?? string.Empty,
            RecommendedLegacyCode = legacyCodeSelection.RecommendedLegacyCode,
            DominantLegacyCode = legacyCodeSelection.DominantLegacyCode,
            RecommendedLegacyCodeCount = legacyCodeSelection.RecommendedLegacyCodeCount,
            RecommendedLegacyCodeShare = legacyCodeSelection.RecommendedLegacyCodeShare,
            LegacyCodeSelectionSummary = legacyCodeSelection.Summary,
            LegacyCodeDistributions = legacyCodeSelection.Distributions
                .Select(MapLegacyCodeDistribution)
                .ToArray(),
            RequiresRatedParams = dependencyProfile?.RequiresRatedParams == true,
            UpstreamCanonicalCodes = dependencyProfile?.UpstreamCanonicalCodes ?? Array.Empty<string>(),
            UpstreamLegacyAliases = upstreamLegacyAliases,
            UpstreamLegacyCodeDistributions = upstreamLegacyCodeDistributions
                .Select(MapUpstreamLegacyCodeDistribution)
                .ToArray(),
            ObservedUpstreamCanonicalCodeCount = upstream.ObservedUpstreamCanonicalCodeCount,
            ObservedUpstreamCanonicalCodes = upstream.ObservedUpstreamCanonicalCodes,
            ObservedUpstreamLegacyCodes = upstream.ObservedUpstreamLegacyCodes,
            MissingUpstreamCanonicalCodes = upstream.MissingUpstreamCanonicalCodes,
            UpstreamDependenciesSatisfied = upstream.UpstreamDependenciesSatisfied,
            UpstreamDependencySummary = upstream.UpstreamDependencySummary,
            RequiredPayloadFields = requiredPayloadFields,
            RequiredRatedParamFields = dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
            RequiredResultFields = dependencyProfile?.RequiredResultFields ?? Array.Empty<string>(),
            RequiredIntermediateResultFields = dependencyProfile?.RequiredIntermediateResultFields ?? Array.Empty<string>(),
            CoveredRequiredIntermediateResultFieldCount = intermediateResultCoverage.CoveredRequiredResultFieldCount,
            MissingRequiredIntermediateResultFieldCount = intermediateResultCoverage.MissingRequiredResultFieldCount,
            MissingRequiredIntermediateResultFields = intermediateResultCoverage.MissingRequiredResultFields,
            CoveredRequiredIntermediateResultFields = intermediateResultCoverage.CoveredRequiredResultFields,
            RequiredIntermediateResultFieldCoverageRatio = intermediateResultCoverage.RequiredResultFieldCoverageRatio,
            RequiredIntermediateResultFieldCoveragePercentagePoints = intermediateResultCoverage.RequiredResultFieldCoveragePercentagePoints,
            RequiredIntermediateResultFieldCoverageSummary = intermediateResultCoverage.RequiredResultFieldCoverageSummary,
            CoveredRequiredResultFieldCount = resultCoverage.CoveredRequiredResultFieldCount,
            MissingRequiredResultFieldCount = resultCoverage.MissingRequiredResultFieldCount,
            MissingRequiredResultFields = resultCoverage.MissingRequiredResultFields,
            CoveredRequiredResultFields = resultCoverage.CoveredRequiredResultFields,
            RequiredResultFieldCoverageRatio = resultCoverage.RequiredResultFieldCoverageRatio,
            RequiredResultFieldCoveragePercentagePoints = resultCoverage.RequiredResultFieldCoveragePercentagePoints,
            RequiredResultFieldCoverageSummary = resultCoverage.RequiredResultFieldCoverageSummary,
            CoveredRequiredPayloadFieldCount = coverage.CoveredRequiredPayloadFieldCount,
            MissingRequiredPayloadFieldCount = coverage.MissingRequiredPayloadFieldCount,
            MissingRequiredPayloadFields = coverage.MissingRequiredPayloadFields,
            CoveredRequiredPayloadFields = coverage.CoveredRequiredPayloadFields,
            RequiredPayloadFieldCoverageRatio = coverage.RequiredPayloadFieldCoverageRatio,
            RequiredPayloadFieldCoveragePercentagePoints = coverage.RequiredPayloadFieldCoveragePercentagePoints,
            SamplePayloadAvailable = coverage.SamplePayloadAvailable,
            RequiredPayloadFieldCoverageSummary = coverage.RequiredPayloadFieldCoverageSummary,
            RequiredRawDataSignals = rawDataSignalCoverage.RequiredSignals,
            ObservedRawDataSignals = rawDataSignalCoverage.ObservedSignals,
            MissingRawDataSignals = rawDataSignalCoverage.MissingSignals,
            RawDataSignalCoveredCount = rawDataSignalCoverage.ObservedSignals.Count,
            RawDataSignalMissingCount = rawDataSignalCoverage.MissingSignals.Count,
            RawDataSampleCount = rawDataSignalCoverage.RawSampleCount,
            RawDataListAvailable = rawDataSignalCoverage.RawDataListAvailable,
            RawDataSignalCoverageRatio = rawDataSignalCoverage.CoverageRatio,
            RawDataSignalCoveragePercentagePoints = rawDataSignalCoverage.CoveragePercentagePoints,
            RawDataSignalCoverageSummary = rawDataSignalCoverage.Summary,
            CoveredRequiredRatedParamFieldCount = ratedCoverage.CoveredRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFieldCount = ratedCoverage.MissingRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFields = ratedCoverage.MissingRequiredRatedParamFields,
            CoveredRequiredRatedParamFields = ratedCoverage.CoveredRequiredRatedParamFields,
            RequiredRatedParamFieldCoverageRatio = ratedCoverage.RequiredRatedParamFieldCoverageRatio,
            RequiredRatedParamFieldCoveragePercentagePoints = ratedCoverage.RequiredRatedParamFieldCoveragePercentagePoints,
            RatedParamsAvailable = ratedCoverage.RatedParamsAvailable,
            RequiredRatedParamFieldCoverageSummary = ratedCoverage.RequiredRatedParamFieldCoverageSummary,
            LegacyAlgorithmInputsReady = legacyAlgorithmInputsReady,
            ObservedAlgorithmInputFields = observedAlgorithmInputFields,
            MissingAlgorithmInputFields = missingAlgorithmInputFields,
            ObservedAlgorithmInputFieldCount = observedAlgorithmInputFields.Length,
            MissingAlgorithmInputFieldCount = missingAlgorithmInputFields.Length,
            AlgorithmInputFieldCoverageRatio = algorithmInputFieldCoverageRatio,
            AlgorithmInputFieldCoveragePercentagePoints = algorithmInputFieldCoveragePercentagePoints,
            AlgorithmInputFieldCoverageSummary = algorithmInputFieldCoverageSummary,
            RawDataSignalsReady = rawDataSignalsReady,
            RequiredStructuredPayloadSignals = dependencyProfile?.RequiredStructuredPayloadSignals ?? Array.Empty<string>(),
            ObservedStructuredPayloadSignals = structuredPayloadCoverage.ObservedSignals,
            MissingStructuredPayloadSignals = structuredPayloadCoverage.MissingSignals,
            StructuredPayloadSignalCoveredCount = structuredPayloadCoverage.CoveredSignalCount,
            StructuredPayloadSignalMissingCount = structuredPayloadCoverage.MissingSignalCount,
            StructuredPayloadSampleCount = structuredPayloadCoverage.SampleCount,
            StructuredPayloadAvailable = structuredPayloadCoverage.StructuredDataAvailable,
            StructuredPayloadSignalCoverageRatio = structuredPayloadCoverage.CoverageRatio,
            StructuredPayloadSignalCoveragePercentagePoints = structuredPayloadCoverage.CoveragePercentagePoints,
            StructuredPayloadSignalCoverageSummary = structuredPayloadCoverage.Summary,
            RequiredStructuredResultSignals = dependencyProfile?.RequiredStructuredResultSignals ?? Array.Empty<string>(),
            ObservedStructuredResultSignals = structuredResultCoverage.ObservedSignals,
            MissingStructuredResultSignals = structuredResultCoverage.MissingSignals,
            StructuredResultSignalCoveredCount = structuredResultCoverage.CoveredSignalCount,
            StructuredResultSignalMissingCount = structuredResultCoverage.MissingSignalCount,
            StructuredResultSampleCount = structuredResultCoverage.SampleCount,
            StructuredResultAvailable = structuredResultCoverage.StructuredDataAvailable,
            StructuredResultSignalCoverageRatio = structuredResultCoverage.CoverageRatio,
            StructuredResultSignalCoveragePercentagePoints = structuredResultCoverage.CoveragePercentagePoints,
            StructuredResultSignalCoverageSummary = structuredResultCoverage.Summary,
            LegacyAlgorithmInputReadinessSummary = legacyAlgorithmInputReadinessSummary,
            DependencyNotes = dependencyProfile?.Notes ?? string.Empty,
            FormulaSignals = dependencyProfile?.FormulaSignals ?? Array.Empty<string>(),
            CoveredFormulaSignalCount = formulaCoverage.CoveredCount,
            MissingFormulaSignalCount = formulaCoverage.MissingCount,
            CoveredFormulaSignals = formulaCoverage.CoveredItems,
            MissingFormulaSignals = formulaCoverage.MissingItems,
            FormulaSignalCoverageRatio = formulaCoverage.CoverageRatio,
            FormulaSignalCoveragePercentagePoints = formulaCoverage.CoveragePercentagePoints,
            FormulaSignalsBackedByObservedPayload = formulaEvidence.BackedByObservedPayload,
            FormulaSignalsObservedPayloadFields = formulaEvidence.ObservedPayloadFields,
            FormulaSignalObservedPayloadGaps = formulaEvidence.SignalOrRuleGaps.Select(MapEvidenceGap).ToArray(),
            FormulaSignalsObservedPayloadSummary = formulaEvidence.Summary,
            LegacyAlgorithmRules = dependencyProfile?.LegacyAlgorithmRules ?? Array.Empty<string>(),
            CoveredLegacyAlgorithmRuleCount = ruleCoverage.CoveredCount,
            MissingLegacyAlgorithmRuleCount = ruleCoverage.MissingCount,
            CoveredLegacyAlgorithmRules = ruleCoverage.CoveredItems,
            MissingLegacyAlgorithmRules = ruleCoverage.MissingItems,
            LegacyAlgorithmRuleCoverageRatio = ruleCoverage.CoverageRatio,
            LegacyAlgorithmRuleCoveragePercentagePoints = ruleCoverage.CoveragePercentagePoints,
            LegacyAlgorithmRulesBackedByObservedPayload = ruleEvidence.BackedByObservedPayload,
            LegacyAlgorithmRulesObservedPayloadFields = ruleEvidence.ObservedPayloadFields,
            LegacyAlgorithmRulesObservedPayloadGaps = ruleEvidence.SignalOrRuleGaps.Select(MapEvidenceGap).ToArray(),
            LegacyAlgorithmRulesObservedPayloadSummary = ruleEvidence.Summary,
            LegacyDecisionAnchors = dependencyProfile?.LegacyDecisionAnchors ?? Array.Empty<string>(),
            CoveredLegacyDecisionAnchorCount = decisionAnchorCoverage.CoveredCount,
            MissingLegacyDecisionAnchorCount = decisionAnchorCoverage.MissingCount,
            CoveredLegacyDecisionAnchors = decisionAnchorCoverage.CoveredItems,
            MissingLegacyDecisionAnchors = decisionAnchorCoverage.MissingItems,
            LegacyDecisionAnchorCoverageRatio = decisionAnchorCoverage.CoverageRatio,
            LegacyDecisionAnchorCoveragePercentagePoints = decisionAnchorCoverage.CoveragePercentagePoints,
            LegacyDecisionAnchorsBackedByObservedPayload = decisionAnchorEvidence.BackedByObservedPayload,
            LegacyDecisionAnchorsObservedPayloadFields = decisionAnchorEvidence.ObservedPayloadFields,
            LegacyDecisionAnchorsObservedPayloadGaps = decisionAnchorEvidence.SignalOrRuleGaps.Select(MapEvidenceGap).ToArray(),
            LegacyDecisionAnchorsObservedPayloadSummary = decisionAnchorEvidence.Summary,
            FormulaSignalSummary = formulaCoverage.Summary,
            LegacyAlgorithmRuleSummary = ruleCoverage.Summary,
            LegacyDecisionAnchorSummary = decisionAnchorCoverage.Summary,
            SelectedMethodSummary = selection.SelectedMethodSummary,
            BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
            DependencyBuckets = dependencyBuckets.Select(MapDependencyBucket).ToArray(),
            Distributions = selection.Distributions
                .Select(MapDistribution)
                .ToArray()
        };
    }

    private static string BuildLegacyAlgorithmInputReadinessSummary(
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        bool legacyAlgorithmInputsReady)
    {
        var payloadStatus = payloadCoverage.RequiredPayloadFieldCoverageSummary;
        var ratedStatus = ratedCoverage.RequiredRatedParamFieldCoverageSummary;
        var resultStatus = resultCoverage.RequiredResultFieldCoverageSummary;
        var intermediateResultStatus = intermediateResultCoverage.RequiredResultFieldCoverageSummary;
        var upstreamStatus = upstream.UpstreamDependencySummary;
        var rawDataStatus = rawDataCoverage.Summary;
        var structuredPayloadStatus = structuredPayloadCoverage.Summary;
        var structuredResultStatus = structuredResultCoverage.Summary;

        return legacyAlgorithmInputsReady
            ? $"legacy algorithm inputs ready; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {intermediateResultStatus}; {rawDataStatus}; {structuredPayloadStatus}; {structuredResultStatus}"
            : $"legacy algorithm inputs incomplete; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {intermediateResultStatus}; {rawDataStatus}; {structuredPayloadStatus}; {structuredResultStatus}";
    }

    private static MotorYLegacyUpstreamCodeDistributionContract MapUpstreamLegacyCodeDistribution(MotorYLegacyUpstreamCodeDistributionSnapshot snapshot)
    {
        return new MotorYLegacyUpstreamCodeDistributionContract
        {
            CanonicalCode = snapshot.CanonicalCode,
            LegacyCode = snapshot.LegacyCode,
            Count = snapshot.Count,
            Share = snapshot.Share
        };
    }

    private static MotorYObservedAlgorithmEvidenceGapContract MapEvidenceGap(MotorYObservedAlgorithmEvidenceGap gap)
    {
        return new MotorYObservedAlgorithmEvidenceGapContract
        {
            SignalOrRule = gap.SignalOrRule,
            RequiredPayloadFields = gap.RequiredPayloadFields,
            ObservedPayloadFields = gap.ObservedPayloadFields,
            MissingPayloadFields = gap.MissingPayloadFields,
            CoveredByObservedPayload = gap.CoveredByObservedPayload,
            Summary = gap.Summary
        };
    }

    private static MotorYDependencyBucketSummaryContract MapDependencyBucket(MotorYDependencyBucketSummarySnapshot snapshot)
    {
        return new MotorYDependencyBucketSummaryContract
        {
            BucketKey = snapshot.BucketKey,
            DisplayName = snapshot.DisplayName,
            RequiredCount = snapshot.RequiredCount,
            CoveredCount = snapshot.CoveredCount,
            MissingCount = snapshot.MissingCount,
            CoverageRatio = snapshot.CoverageRatio,
            CoveragePercentagePoints = snapshot.CoveragePercentagePoints,
            RequiredItems = snapshot.RequiredItems,
            CoveredItems = snapshot.CoveredItems,
            MissingItems = snapshot.MissingItems,
            Summary = snapshot.Summary
        };
    }

    private static MotorYMethodDistributionContract MapDistribution(MotorYMethodDistributionSnapshot snapshot)
    {
        return new MotorYMethodDistributionContract
        {
            MethodValue = snapshot.MethodValue,
            Count = snapshot.Count,
            Share = snapshot.Share,
            Profile = MapBuildProfile(snapshot.Route)
        };
    }

    private static MotorYLegacyCodeDistributionContract MapLegacyCodeDistribution(MotorYLegacyCodeDistributionSnapshot snapshot)
    {
        return new MotorYLegacyCodeDistributionContract
        {
            CanonicalCode = snapshot.CanonicalCode,
            LegacyCode = snapshot.LegacyCode,
            Count = snapshot.Count,
            Share = snapshot.Share
        };
    }

    private static MotorYLegacyCodeSelectionSnapshot BuildLegacyCodeSelection(
        string canonicalCode,
        IReadOnlyList<MotorYLegacyCodeDistributionSnapshot>? legacyCodeDistributions)
    {
        var distributions = (legacyCodeDistributions ?? Array.Empty<MotorYLegacyCodeDistributionSnapshot>())
            .Where(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();

        if (distributions.Length == 0)
        {
            return new MotorYLegacyCodeSelectionSnapshot
            {
                CanonicalCode = canonicalCode,
                Summary = "legacy code selection unavailable in builder-only route planning"
            };
        }

        var recommended = distributions[0];
        return new MotorYLegacyCodeSelectionSnapshot
        {
            CanonicalCode = canonicalCode,
            RecommendedLegacyCode = recommended.LegacyCode,
            DominantLegacyCode = recommended.LegacyCode,
            RecommendedLegacyCodeCount = recommended.Count,
            RecommendedLegacyCodeShare = recommended.Share,
            Distributions = distributions,
            Summary = $"recommended legacy code '{recommended.LegacyCode}' for {canonicalCode} ({recommended.Count}/{distributions.Sum(x => x.Count)}, {(int)Math.Round(recommended.Share * 100d, MidpointRounding.AwayFromZero)}pp)"
        };
    }

    private static MotorYBuildProfileContract? MapBuildProfile(MotorYLegacyAlgorithmRoute? route)
    {
        return route is null
            ? null
            : new MotorYBuildProfileContract
            {
                CanonicalCode = route.CanonicalCode,
                MethodValue = route.MethodValue,
                MethodKey = route.MethodKey,
                ProfileKey = route.ProfileKey,
                VariantKind = route.VariantKind,
                AlgorithmFamily = route.AlgorithmFamily,
                LegacyEnumName = route.LegacyEnumName,
                LegacyFormName = route.LegacyFormName,
                LegacyAlgorithmEntry = route.LegacyAlgorithmEntry,
                LegacyMethodName = route.LegacyMethodName,
                LegacySettingsMethodName = route.LegacySettingsMethodName,
                IsBaselineMethod = route.IsBaselineMethod
            };
    }
}
