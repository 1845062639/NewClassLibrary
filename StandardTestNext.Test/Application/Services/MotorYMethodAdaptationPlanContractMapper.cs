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
        var formDependencyEvidences = dependencyProfile?.FormDependencyEvidences.Select(x => new MotorYLegacyFormDependencyEvidenceContract
        {
            FormName = x.FormName,
            SourceFile = x.SourceFile,
            Line = x.Line,
            SourceRange = x.SourceRange,
            SourceAnchor = x.SourceAnchor,
            UpstreamCanonicalCodes = x.UpstreamCanonicalCodes,
            ReferencedMethods = x.ReferencedMethods,
            Summary = x.Summary
        }).ToArray() ?? Array.Empty<MotorYLegacyFormDependencyEvidenceContract>();
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
        var decisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            selection.CanonicalCode,
            decisionAnchorObservedFields,
            observedStructuredSignals);
        var decisionAnchorObservationGaps = decisionAnchorObservationRules
            .Select(rule => new MotorYObservedAlgorithmEvidenceGap
            {
                SignalOrRule = $"decision-anchor-observation:{rule.AnchorKey}",
                RequiredPayloadFields = rule.RequiredPayloadFields,
                ObservedPayloadFields = rule.ObservedPayloadFields,
                MissingPayloadFields = rule.MissingPayloadFields,
                CoveredByObservedPayload = rule.CoveredByObservedPayload,
                Summary = rule.Summary
            })
            .ToArray();
        var decisionAnchorCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
            dependencyProfile?.LegacyDecisionAnchors,
            decisionAnchorEvidence.ObservedPayloadFields,
            "legacy decision anchors");
        var decisionAnchorResolutions = MotorYDecisionAnchorResolutionFactory.Build(selection.CanonicalCode, decisionAnchorObservationRules);
        var resolvedDecisionAnchorCount = decisionAnchorResolutions.Count(x => x.ResolvedByObservedPayload);
        var partialDecisionAnchorCount = decisionAnchorResolutions.Count(x => x.PartiallyResolvedByObservedPayload);
        var missingDecisionAnchorResolutionCount = decisionAnchorResolutions.Count - resolvedDecisionAnchorCount - partialDecisionAnchorCount;
        var decisionAnchorResolutionCoverageRatio = decisionAnchorResolutions.Count == 0
            ? 1d
            : Math.Round((double)resolvedDecisionAnchorCount / decisionAnchorResolutions.Count, 4, MidpointRounding.AwayFromZero);
        var decisionAnchorResolutionCoveragePercentagePoints = decisionAnchorResolutions.Count == 0
            ? 100
            : (int)Math.Round((double)resolvedDecisionAnchorCount / decisionAnchorResolutions.Count * 100d, MidpointRounding.AwayFromZero);
        var decisionAnchorResolutionSummary = MotorYDecisionAnchorResolutionFactory.BuildSummary(decisionAnchorResolutions);
        var decisionAnchorNextActionSummary = MotorYDecisionAnchorResolutionFactory.BuildNextActionSummary(decisionAnchorResolutions);
        var decisionAnchorGapPreviewSummary = MotorYDecisionAnchorResolutionFactory.BuildGapPreviewSummary(decisionAnchorResolutions);
        var decisionAnchorPriorityDistributions = MotorYDecisionAnchorResolutionFactory.BuildPriorityDistributions(decisionAnchorResolutions);
        var decisionAnchorPrioritySummary = MotorYDecisionAnchorResolutionFactory.BuildPrioritySummary(decisionAnchorResolutions);
        var topDecisionAnchorPriority = MotorYDecisionAnchorResolutionFactory.BuildTopPriorityDistribution(decisionAnchorResolutions);
        var suggestedDecisionAnchorNextSteps = MotorYDecisionAnchorResolutionFactory.BuildSuggestedNextSteps(decisionAnchorResolutions);
        var suggestedDecisionAnchorNextStepSummary = suggestedDecisionAnchorNextSteps.Count == 0
            ? "no decision-anchor next-step recommendation"
            : string.Join("; ", suggestedDecisionAnchorNextSteps);
        var legacyDecisionAnchorReady = missingDecisionAnchorResolutionCount == 0;
        var minimumRawSampleCount = dependencyProfile?.MinimumRawSampleCount ?? 0;
        var rawSampleCountReady = rawDataSignalCoverage.RawSampleCount >= minimumRawSampleCount;
        var rawSampleCountGap = Math.Max(0, minimumRawSampleCount - rawDataSignalCoverage.RawSampleCount);
        var rawSampleCountSummary = minimumRawSampleCount <= 0
            ? $"raw sample count requirement not set; observed {rawDataSignalCoverage.RawSampleCount}"
            : rawSampleCountReady
                ? $"raw sample count ready {rawDataSignalCoverage.RawSampleCount}/{minimumRawSampleCount}"
                : $"raw sample count insufficient {rawDataSignalCoverage.RawSampleCount}/{minimumRawSampleCount}";
        var rawSampleCountDecisionSummary = minimumRawSampleCount <= 0
            ? $"raw sample count gate disabled for {selection.CanonicalCode}; observed {rawDataSignalCoverage.RawSampleCount}"
            : rawSampleCountReady
                ? $"raw sample count gate passed for {selection.CanonicalCode}: observed {rawDataSignalCoverage.RawSampleCount} >= required {minimumRawSampleCount}"
                : $"raw sample count gate blocked for {selection.CanonicalCode}: observed {rawDataSignalCoverage.RawSampleCount}, still need {rawSampleCountGap} more samples to reach {minimumRawSampleCount}";
        var minimumStructuredPayloadSampleCount = dependencyProfile?.MinimumStructuredPayloadSampleCount ?? 0;
        var structuredPayloadSampleCountReady = structuredPayloadCoverage.SampleCount >= minimumStructuredPayloadSampleCount;
        var structuredPayloadSampleCountGap = Math.Max(0, minimumStructuredPayloadSampleCount - structuredPayloadCoverage.SampleCount);
        var structuredPayloadSampleCountSummary = minimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count requirement not set; observed {structuredPayloadCoverage.SampleCount}"
            : structuredPayloadSampleCountReady
                ? $"structured payload sample count ready {structuredPayloadCoverage.SampleCount}/{minimumStructuredPayloadSampleCount}"
                : $"structured payload sample count insufficient {structuredPayloadCoverage.SampleCount}/{minimumStructuredPayloadSampleCount}";
        var structuredPayloadSampleCountDecisionSummary = minimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count gate disabled for {selection.CanonicalCode}; observed {structuredPayloadCoverage.SampleCount}"
            : structuredPayloadSampleCountReady
                ? $"structured payload sample count gate passed for {selection.CanonicalCode}: observed {structuredPayloadCoverage.SampleCount} >= required {minimumStructuredPayloadSampleCount}"
                : $"structured payload sample count gate blocked for {selection.CanonicalCode}: observed {structuredPayloadCoverage.SampleCount}, still need {structuredPayloadSampleCountGap} more samples to reach {minimumStructuredPayloadSampleCount}";
        var minimumStructuredResultSampleCount = dependencyProfile?.MinimumStructuredResultSampleCount ?? 0;
        var structuredResultSampleCountReady = structuredResultCoverage.SampleCount >= minimumStructuredResultSampleCount;
        var structuredResultSampleCountGap = Math.Max(0, minimumStructuredResultSampleCount - structuredResultCoverage.SampleCount);
        var structuredResultSampleCountSummary = minimumStructuredResultSampleCount <= 0
            ? $"structured result sample count requirement not set; observed {structuredResultCoverage.SampleCount}"
            : structuredResultSampleCountReady
                ? $"structured result sample count ready {structuredResultCoverage.SampleCount}/{minimumStructuredResultSampleCount}"
                : $"structured result sample count insufficient {structuredResultCoverage.SampleCount}/{minimumStructuredResultSampleCount}";
        var structuredResultSampleCountDecisionSummary = minimumStructuredResultSampleCount <= 0
            ? $"structured result sample count gate disabled for {selection.CanonicalCode}; observed {structuredResultCoverage.SampleCount}"
            : structuredResultSampleCountReady
                ? $"structured result sample count gate passed for {selection.CanonicalCode}: observed {structuredResultCoverage.SampleCount} >= required {minimumStructuredResultSampleCount}"
                : $"structured result sample count gate blocked for {selection.CanonicalCode}: observed {structuredResultCoverage.SampleCount}, still need {structuredResultSampleCountGap} more samples to reach {minimumStructuredResultSampleCount}";
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
        var observedAlgorithmInputFieldSources = MotorYObservedFieldSourceCatalog.Build(
            coverage.CoveredRequiredPayloadFields,
            ratedCoverage.CoveredRequiredRatedParamFields,
            resultCoverage.CoveredRequiredResultFields,
            intermediateResultCoverage.CoveredRequiredResultFields,
            rawDataSignalCoverage.ObservedSignals,
            structuredPayloadCoverage.ObservedSignals,
            structuredResultCoverage.ObservedSignals);
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
            && structuredSignalsReady
            && rawSampleCountReady
            && structuredPayloadSampleCountReady
            && structuredResultSampleCountReady
            && legacyDecisionAnchorReady;
        var legacyAlgorithmInputReadinessSummary = BuildLegacyAlgorithmInputReadinessSummary(
            upstream,
            coverage,
            ratedCoverage,
            resultCoverage,
            intermediateResultCoverage,
            rawDataSignalCoverage,
            structuredPayloadCoverage,
            structuredResultCoverage,
            rawSampleCountReady,
            rawSampleCountSummary,
            structuredPayloadSampleCountReady,
            structuredPayloadSampleCountSummary,
            structuredResultSampleCountReady,
            structuredResultSampleCountSummary,
            legacyDecisionAnchorReady,
            decisionAnchorResolutionSummary,
            legacyAlgorithmInputsReady);
        var suggestedNextSteps = BuildSuggestedNextSteps(
            selection.CanonicalCode,
            upstream,
            coverage,
            ratedCoverage,
            resultCoverage,
            intermediateResultCoverage,
            rawDataSignalCoverage,
            structuredPayloadCoverage,
            structuredResultCoverage,
            decisionAnchorResolutions);
        var suggestedNextStepSummary = suggestedNextSteps.Count == 0
            ? "no immediate next-step recommendation"
            : string.Join("; ", suggestedNextSteps);

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
            decisionAnchorCoverage,
            decisionAnchorResolutions);

        var topDecisionAnchorResolution = decisionAnchorResolutions
            .FirstOrDefault(resolution => string.Equals(resolution.AnchorKey, topDecisionAnchorPriority?.DominantAnchorKey, StringComparison.Ordinal));
        var decisionAnchorPrimaryFieldDistributions = MotorYDecisionAnchorResolutionFactory.BuildPrimaryFieldDistributions(decisionAnchorResolutions)
            .Select(distribution => new MotorYDecisionAnchorPrimaryFieldDistributionContract
            {
                PrimaryField = distribution.PrimaryField,
                Count = distribution.Count,
                Share = distribution.Share,
                AnchorKeys = distribution.AnchorKeys,
                SuggestedNextStepFocuses = distribution.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = distribution.SuggestedNextStepPriorities,
                CanonicalCodes = new[] { selection.CanonicalCode },
                Summary = distribution.Summary
            })
            .ToArray();
        var requiredResultPrimaryFieldDistributions = BuildRequiredResultPrimaryFieldDistributions(resultCoverage, intermediateResultCoverage);
        var requiredResultPrimaryFieldSummary = BuildRequiredResultPrimaryFieldSummary(requiredResultPrimaryFieldDistributions);
        var crossPlanDecisionAnchorPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string crossPlanDecisionAnchorPrimaryFieldSummary = "cross-plan decision-anchor primary fields: pending cross-plan aggregation";
        var algorithmFamilyDecisionAnchorPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string algorithmFamilyDecisionAnchorPrimaryFieldSummary = "algorithm-family decision-anchor primary fields: pending family aggregation";
        var variantKindDecisionAnchorPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string variantKindDecisionAnchorPrimaryFieldSummary = "variant-kind decision-anchor primary fields: pending variant aggregation";
        var crossPlanRequiredResultPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string crossPlanRequiredResultPrimaryFieldSummary = "cross-plan required-result primary fields: pending cross-plan aggregation";
        var algorithmFamilyRequiredResultPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string algorithmFamilyRequiredResultPrimaryFieldSummary = "algorithm-family required-result primary fields: pending family aggregation";
        var variantKindRequiredResultPrimaryFieldFocuses = Array.Empty<MotorYPrimaryFieldFocusContract>();
        const string variantKindRequiredResultPrimaryFieldSummary = "variant-kind required-result primary fields: pending variant aggregation";

        return new MotorYMethodAdaptationPlanContract
        {
            CanonicalCode = selection.CanonicalCode,
            DecisionAnchorTopPriority = topDecisionAnchorPriority?.Priority ?? string.Empty,
            DecisionAnchorTopPrioritySummary = topDecisionAnchorPriority is null
                ? "decision anchor top priority unavailable"
                : $"top decision anchor priority={topDecisionAnchorPriority.Priority}; focus={topDecisionAnchorPriority.DominantSuggestedNextStepFocus}; anchor={topDecisionAnchorPriority.DominantAnchorKey}; fields={(topDecisionAnchorPriority.DominantSuggestedNextStepFields.Count == 0 ? "none" : string.Join(", ", topDecisionAnchorPriority.DominantSuggestedNextStepFields))}",
            DecisionAnchorTopPriorityDominantAnchorKey = topDecisionAnchorPriority?.DominantAnchorKey ?? string.Empty,
            DecisionAnchorTopPriorityFocus = topDecisionAnchorPriority?.DominantSuggestedNextStepFocus ?? string.Empty,
            DecisionAnchorTopPriorityFields = topDecisionAnchorPriority?.DominantSuggestedNextStepFields ?? Array.Empty<string>(),
            DecisionAnchorTopPriorityNextStepSummary = topDecisionAnchorPriority?.DominantSuggestedNextStepSummary ?? string.Empty,
            DecisionAnchorTopPriorityPrimaryField = topDecisionAnchorResolution?.SuggestedPrimaryNextField ?? string.Empty,
            DecisionAnchorTopPriorityPrimaryFieldSummary = topDecisionAnchorResolution?.SuggestedPrimaryNextFieldSummary ?? string.Empty,
            DecisionAnchorTopPriorityDetail = topDecisionAnchorPriority is null
                ? null
                : new MotorYDecisionAnchorTopPriorityContract
                {
                    Priority = topDecisionAnchorPriority.Priority,
                    AnchorKey = topDecisionAnchorPriority.DominantAnchorKey,
                    Focus = topDecisionAnchorPriority.DominantSuggestedNextStepFocus,
                    Fields = topDecisionAnchorPriority.DominantSuggestedNextStepFields,
                    NextStepSummary = topDecisionAnchorPriority.DominantSuggestedNextStepSummary,
                    PrimaryField = topDecisionAnchorResolution?.SuggestedPrimaryNextField ?? string.Empty,
                    PrimaryFieldSummary = topDecisionAnchorResolution?.SuggestedPrimaryNextFieldSummary ?? string.Empty,
                    Summary = topDecisionAnchorPriority is null
                        ? "decision anchor top priority unavailable"
                        : $"top decision anchor priority={topDecisionAnchorPriority.Priority}; focus={topDecisionAnchorPriority.DominantSuggestedNextStepFocus}; anchor={topDecisionAnchorPriority.DominantAnchorKey}; fields={(topDecisionAnchorPriority.DominantSuggestedNextStepFields.Count == 0 ? "none" : string.Join(", ", topDecisionAnchorPriority.DominantSuggestedNextStepFields))}"
                },
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
            AlgorithmFamily = selectedProfile?.AlgorithmFamily ?? string.Empty,
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
            Distributions = snapshot.Distributions
                .Select(x => new MotorYMethodDistributionContract
                {
                    MethodValue = x.MethodValue,
                    Count = x.Count,
                    Share = x.Share,
                    Profile = profileMapper(x.Route)
                })
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
            SourceEvidences = (dependencyProfile?.SourceEvidences ?? Array.Empty<MotorYLegacyAlgorithmSourceEvidence>())
                .Select(evidence => new MotorYLegacyAlgorithmSourceEvidenceContract
                {
                    SectionKey = evidence.SectionKey,
                    MethodName = evidence.MethodName,
                    SourceFile = evidence.SourceFile,
                    StartLine = evidence.StartLine,
                    EndLine = evidence.EndLine,
                    SourceRange = evidence.SourceRange,
                    SourceAnchor = evidence.SourceAnchor,
                    ReferencedFields = evidence.ReferencedFields,
                    Summary = evidence.Summary
                })
                .ToArray(),
            FormDependencyEvidences = formDependencyEvidences,
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
            ObservedAlgorithmInputFieldSources = observedAlgorithmInputFieldSources
                .Select(x => new MotorYObservedFieldSourceContract
                {
                    FieldName = x.FieldName,
                    SourceType = x.SourceType,
                    SourceScope = x.SourceScope,
                    SourceSummary = x.SourceSummary
                })
                .ToArray(),
            MissingAlgorithmInputFields = missingAlgorithmInputFields,
            ObservedAlgorithmInputFieldCount = observedAlgorithmInputFields.Length,
            MissingAlgorithmInputFieldCount = missingAlgorithmInputFields.Length,
            AlgorithmInputFieldCoverageRatio = algorithmInputFieldCoverageRatio,
            AlgorithmInputFieldCoveragePercentagePoints = algorithmInputFieldCoveragePercentagePoints,
            AlgorithmInputFieldCoverageSummary = algorithmInputFieldCoverageSummary,
            RawDataSignalsReady = rawDataSignalsReady,
            MinimumRawSampleCount = minimumRawSampleCount,
            RawSampleCountReady = rawSampleCountReady,
            RawSampleCountReadinessSummary = rawSampleCountSummary,
            RawSampleCountGap = rawSampleCountGap,
            RawSampleCountDecisionSummary = rawSampleCountDecisionSummary,
            RequiredStructuredPayloadSignals = dependencyProfile?.RequiredStructuredPayloadSignals ?? Array.Empty<string>(),
            MinimumStructuredPayloadSampleCount = minimumStructuredPayloadSampleCount,
            StructuredPayloadSampleCountReady = structuredPayloadSampleCountReady,
            StructuredPayloadSampleCountReadinessSummary = structuredPayloadSampleCountSummary,
            StructuredPayloadSampleCountGap = structuredPayloadSampleCountGap,
            StructuredPayloadSampleCountDecisionSummary = structuredPayloadSampleCountDecisionSummary,
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
            MinimumStructuredResultSampleCount = minimumStructuredResultSampleCount,
            StructuredResultSampleCountReady = structuredResultSampleCountReady,
            StructuredResultSampleCountReadinessSummary = structuredResultSampleCountSummary,
            StructuredResultSampleCountGap = structuredResultSampleCountGap,
            StructuredResultSampleCountDecisionSummary = structuredResultSampleCountDecisionSummary,
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
            SuggestedNextSteps = suggestedNextSteps,
            SuggestedNextStepSummary = suggestedNextStepSummary,
            FormulaSignals = dependencyProfile?.FormulaSignals ?? Array.Empty<string>(),
            CoveredFormulaSignalCount = formulaCoverage.CoveredCount,
            MissingFormulaSignalCount = formulaCoverage.MissingCount,
            CoveredFormulaSignals = formulaCoverage.CoveredItems,
            MissingFormulaSignals = formulaCoverage.MissingItems,
            FormulaSignalCoverageRatio = formulaCoverage.CoverageRatio,
            FormulaSignalCoveragePercentagePoints = formulaCoverage.CoveragePercentagePoints,
            FormulaSignalsBackedByObservedPayload = formulaEvidence.BackedByObservedPayload,
            FormulaSignalsObservedPayloadFields = formulaEvidence.ObservedPayloadFields,
            FormulaSignalObservedPayloadGaps = formulaEvidence.SignalOrRuleGaps
                .Select(MapObservedAlgorithmEvidenceGap)
                .ToArray(),
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
            LegacyAlgorithmRulesObservedPayloadGaps = ruleEvidence.SignalOrRuleGaps
                .Select(MapObservedAlgorithmEvidenceGap)
                .ToArray(),
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
            LegacyDecisionAnchorsObservedPayloadGaps = decisionAnchorObservationGaps
                .Select(MapObservedAlgorithmEvidenceGap)
                .ToArray(),
            LegacyDecisionAnchorsObservedPayloadSummary = decisionAnchorEvidence.Summary,
            LegacyDecisionAnchorObservationRules = decisionAnchorObservationRules
                .Select(rule => new MotorYDecisionAnchorObservationRuleContract
                {
                    AnchorKey = rule.AnchorKey,
                    RequiredPayloadFields = rule.RequiredPayloadFields,
                    ObservedPayloadFields = rule.ObservedPayloadFields,
                    MissingPayloadFields = rule.MissingPayloadFields,
                    CoveredByObservedPayload = rule.CoveredByObservedPayload,
                    Summary = rule.Summary
                })
                .ToArray(),
            CoveredLegacyDecisionAnchorObservationRuleCount = decisionAnchorObservationRules.Count(x => x.CoveredByObservedPayload),
            MissingLegacyDecisionAnchorObservationRuleCount = decisionAnchorObservationRules.Count(x => !x.CoveredByObservedPayload),
            LegacyDecisionAnchorObservationRuleCoverageRatio = decisionAnchorObservationRules.Count == 0
                ? 1d
                : Math.Round((double)decisionAnchorObservationRules.Count(x => x.CoveredByObservedPayload) / decisionAnchorObservationRules.Count, 4, MidpointRounding.AwayFromZero),
            LegacyDecisionAnchorObservationRuleCoveragePercentagePoints = decisionAnchorObservationRules.Count == 0
                ? 100
                : (int)Math.Round((double)decisionAnchorObservationRules.Count(x => x.CoveredByObservedPayload) / decisionAnchorObservationRules.Count * 100d, MidpointRounding.AwayFromZero),
            LegacyDecisionAnchorObservationRuleSummary = $"decision anchor observation rules covered {decisionAnchorObservationRules.Count(x => x.CoveredByObservedPayload)}/{decisionAnchorObservationRules.Count} ({(decisionAnchorObservationRules.Count == 0 ? 100 : (int)Math.Round((double)decisionAnchorObservationRules.Count(x => x.CoveredByObservedPayload) / decisionAnchorObservationRules.Count * 100d, MidpointRounding.AwayFromZero))}pp); missing: {(decisionAnchorObservationRules.Count(x => !x.CoveredByObservedPayload) == 0 ? "none" : string.Join(", ", decisionAnchorObservationRules.Where(x => !x.CoveredByObservedPayload).Select(x => x.AnchorKey)))}",
            LegacyDecisionAnchorReady = legacyDecisionAnchorReady,
            LegacyDecisionAnchorResolutions = decisionAnchorResolutions
                .Select(resolution => new MotorYDecisionAnchorResolutionContract
                {
                    AnchorKey = resolution.AnchorKey,
                    ResolvedByObservedPayload = resolution.ResolvedByObservedPayload,
                    PartiallyResolvedByObservedPayload = resolution.PartiallyResolvedByObservedPayload,
                    RequiredPayloadFields = resolution.RequiredPayloadFields,
                    ObservedPayloadFields = resolution.ObservedPayloadFields,
                    MissingPayloadFields = resolution.MissingPayloadFields,
                    CoverageRatio = resolution.CoverageRatio,
                    CoveragePercentagePoints = resolution.CoveragePercentagePoints,
                    ResolutionStage = resolution.ResolutionStage,
                    SuggestedNextStepCategory = resolution.SuggestedNextStepCategory,
                    SuggestedNextStepFocus = resolution.SuggestedNextStepFocus,
                    SuggestedNextStepFields = resolution.SuggestedNextStepFields,
                    SuggestedNextSteps = resolution.SuggestedNextSteps,
                    SuggestedNextStepSummary = resolution.SuggestedNextStepSummary,
                    SuggestedNextStepPriority = resolution.SuggestedNextStepPriority,
                    SuggestedNextStepPrioritySummary = resolution.SuggestedNextStepPrioritySummary,
                    SuggestedNextStepCoverageSummary = resolution.SuggestedNextStepCoverageSummary,
                    SuggestedPrimaryNextField = resolution.SuggestedPrimaryNextField,
                    SuggestedPrimaryNextFieldSummary = resolution.SuggestedPrimaryNextFieldSummary,
                    Summary = resolution.Summary
                })
                .ToArray(),
            ResolvedLegacyDecisionAnchorCount = resolvedDecisionAnchorCount,
            PartialLegacyDecisionAnchorCount = partialDecisionAnchorCount,
            MissingLegacyDecisionAnchorResolutionCount = missingDecisionAnchorResolutionCount,
            EffectiveLegacyDecisionAnchorCoverageCount = resolvedDecisionAnchorCount + partialDecisionAnchorCount,
            EffectiveLegacyDecisionAnchorGapCount = missingDecisionAnchorResolutionCount,
            LegacyDecisionAnchorResolutionCoverageRatio = decisionAnchorResolutionCoverageRatio,
            LegacyDecisionAnchorResolutionCoveragePercentagePoints = decisionAnchorResolutionCoveragePercentagePoints,
            EffectiveLegacyDecisionAnchorCoverageRatio = decisionAnchorResolutions.Count == 0
                ? 1d
                : Math.Round((double)(resolvedDecisionAnchorCount + partialDecisionAnchorCount) / decisionAnchorResolutions.Count, 4, MidpointRounding.AwayFromZero),
            EffectiveLegacyDecisionAnchorCoveragePercentagePoints = decisionAnchorResolutions.Count == 0
                ? 100
                : (int)Math.Round((double)(resolvedDecisionAnchorCount + partialDecisionAnchorCount) / decisionAnchorResolutions.Count * 100d, MidpointRounding.AwayFromZero),
            LegacyDecisionAnchorResolutionSummary = decisionAnchorResolutionSummary,
            LegacyDecisionAnchorNextActionSummary = decisionAnchorNextActionSummary,
            LegacyDecisionAnchorGapPreviewSummary = decisionAnchorGapPreviewSummary,
            DecisionAnchorPriorityDistributions = decisionAnchorPriorityDistributions
                .Select(distribution => new MotorYDecisionAnchorPriorityDistributionContract
                {
                    Priority = distribution.Priority,
                    Count = distribution.Count,
                    Share = distribution.Share,
                    AnchorKeys = distribution.AnchorKeys,
                    SuggestedNextStepFocuses = distribution.SuggestedNextStepFocuses,
                    SuggestedNextStepFields = distribution.SuggestedNextStepFields,
                    SuggestedNextSteps = distribution.SuggestedNextSteps,
                    SuggestedNextStepSummary = distribution.SuggestedNextStepSummary,
                    DominantAnchorKey = distribution.DominantAnchorKey,
                    DominantSuggestedNextStepFocus = distribution.DominantSuggestedNextStepFocus,
                    DominantSuggestedNextStepFields = distribution.DominantSuggestedNextStepFields,
                    DominantSuggestedNextStepSummary = distribution.DominantSuggestedNextStepSummary
                })
                .ToArray(),
            DecisionAnchorPrimaryFieldDistributions = decisionAnchorPrimaryFieldDistributions,
            DecisionAnchorPrimaryFieldSummary = MotorYDecisionAnchorResolutionFactory.BuildPrimaryFieldSummary(decisionAnchorResolutions),
            CrossPlanDecisionAnchorPrimaryFieldFocuses = crossPlanDecisionAnchorPrimaryFieldFocuses,
            CrossPlanDecisionAnchorPrimaryFieldSummary = crossPlanDecisionAnchorPrimaryFieldSummary,
            AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses = algorithmFamilyDecisionAnchorPrimaryFieldFocuses,
            AlgorithmFamilyDecisionAnchorPrimaryFieldSummary = algorithmFamilyDecisionAnchorPrimaryFieldSummary,
            VariantKindDecisionAnchorPrimaryFieldFocuses = variantKindDecisionAnchorPrimaryFieldFocuses,
            VariantKindDecisionAnchorPrimaryFieldSummary = variantKindDecisionAnchorPrimaryFieldSummary,
            RequiredResultPrimaryFieldDistributions = requiredResultPrimaryFieldDistributions,
            RequiredResultPrimaryFieldSummary = requiredResultPrimaryFieldSummary,
            CrossPlanRequiredResultPrimaryFieldFocuses = crossPlanRequiredResultPrimaryFieldFocuses,
            CrossPlanRequiredResultPrimaryFieldSummary = crossPlanRequiredResultPrimaryFieldSummary,
            AlgorithmFamilyRequiredResultPrimaryFieldFocuses = algorithmFamilyRequiredResultPrimaryFieldFocuses,
            AlgorithmFamilyRequiredResultPrimaryFieldSummary = algorithmFamilyRequiredResultPrimaryFieldSummary,
            DecisionAnchorPrioritySummary = decisionAnchorPrioritySummary,
            SuggestedDecisionAnchorNextSteps = suggestedDecisionAnchorNextSteps,
            SuggestedDecisionAnchorNextStepSummary = suggestedDecisionAnchorNextStepSummary,
            SelectedMethodSummary = selection.SelectedMethodSummary,
            BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
            DependencyBuckets = dependencyBuckets.Select(MapDependencyBucket).ToArray()
        };
    }

    private static MotorYObservedAlgorithmEvidenceGapContract MapObservedAlgorithmEvidenceGap(MotorYObservedAlgorithmEvidenceGap gap)
        => new()
        {
            SignalOrRule = gap.SignalOrRule,
            RequiredPayloadFields = gap.RequiredPayloadFields,
            ObservedPayloadFields = gap.ObservedPayloadFields,
            MissingPayloadFields = gap.MissingPayloadFields,
            CoveredByObservedPayload = gap.CoveredByObservedPayload,
            Summary = gap.Summary
        };

    private static MotorYDependencyBucketSummaryContract MapDependencyBucket(MotorYDependencyBucketSummarySnapshot bucket)
        => new()
        {
            BucketKey = bucket.BucketKey,
            DisplayName = bucket.DisplayName,
            RequiredCount = bucket.RequiredCount,
            CoveredCount = bucket.CoveredCount,
            MissingCount = bucket.MissingCount,
            CoveredItems = bucket.CoveredItems,
            MissingItems = bucket.MissingItems,
            CoverageRatio = bucket.CoverageRatio,
            CoveragePercentagePoints = bucket.CoveragePercentagePoints,
            Summary = bucket.Summary
        };

    private static MotorYLegacyCodeDistributionContract MapLegacyCodeDistribution(MotorYLegacyCodeDistributionSnapshot snapshot)
        => new()
        {
            CanonicalCode = snapshot.CanonicalCode,
            LegacyCode = snapshot.LegacyCode,
            Count = snapshot.Count,
            Share = snapshot.Share
        };

    private static MotorYLegacyUpstreamCodeDistributionContract MapUpstreamLegacyCodeDistribution(MotorYLegacyUpstreamCodeDistributionSnapshot snapshot)
        => new()
        {
            CanonicalCode = snapshot.CanonicalCode,
            LegacyCode = snapshot.LegacyCode,
            Count = snapshot.Count,
            Share = snapshot.Share
        };

    private static string BuildLegacyAlgorithmInputReadinessSummary(
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        bool rawSampleCountReady,
        string rawSampleCountSummary,
        bool structuredPayloadSampleCountReady,
        string structuredPayloadSampleCountSummary,
        bool structuredResultSampleCountReady,
        string structuredResultSampleCountSummary,
        bool legacyDecisionAnchorReady,
        string decisionAnchorResolutionSummary,
        bool legacyAlgorithmInputsReady)
    {
        var parts = new List<string>
        {
            upstream.UpstreamDependencySummary,
            payloadCoverage.RequiredPayloadFieldCoverageSummary,
            ratedCoverage.RequiredRatedParamFieldCoverageSummary,
            resultCoverage.RequiredResultFieldCoverageSummary,
            intermediateResultCoverage.RequiredResultFieldCoverageSummary,
            rawDataCoverage.Summary,
            structuredPayloadCoverage.Summary,
            structuredResultCoverage.Summary,
            rawSampleCountReady ? rawSampleCountSummary : rawSampleCountSummary,
            structuredPayloadSampleCountReady ? structuredPayloadSampleCountSummary : structuredPayloadSampleCountSummary,
            structuredResultSampleCountReady ? structuredResultSampleCountSummary : structuredResultSampleCountSummary,
            legacyDecisionAnchorReady
                ? $"decision anchor ready; {decisionAnchorResolutionSummary}"
                : $"decision anchor incomplete; {decisionAnchorResolutionSummary}"
        };

        return legacyAlgorithmInputsReady
            ? $"legacy algorithm inputs ready; {string.Join("; ", parts)}"
            : $"legacy algorithm inputs incomplete; {string.Join("; ", parts)}";
    }

    private sealed class MotorYLegacyCodeSelectionSnapshot
    {
        public string CanonicalCode { get; init; } = string.Empty;
        public string RecommendedLegacyCode { get; init; } = string.Empty;
        public string DominantLegacyCode { get; init; } = string.Empty;
        public int RecommendedLegacyCodeCount { get; init; }
        public double RecommendedLegacyCodeShare { get; init; }
        public IReadOnlyList<MotorYLegacyCodeDistributionSnapshot> Distributions { get; init; } = Array.Empty<MotorYLegacyCodeDistributionSnapshot>();
        public string Summary { get; init; } = string.Empty;
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

    private static IReadOnlyList<string> BuildSuggestedNextSteps(
        string canonicalCode,
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        IReadOnlyList<MotorYDecisionAnchorResolution> decisionAnchorResolutions)
    {
        var steps = new List<string>();

        var unresolvedAnchors = decisionAnchorResolutions
            .Where(x => !x.ResolvedByObservedPayload)
            .Select(x => x.AnchorKey)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (unresolvedAnchors.Length > 0)
        {
            steps.Add($"先补决策锚点观测依据: {FormatPreview(unresolvedAnchors, 3)}");
        }

        if (intermediateResultCoverage.MissingRequiredResultFields.Count > 0)
        {
            steps.Add($"优先回填中间结果字段: {FormatPreview(intermediateResultCoverage.MissingRequiredResultFields, 4)}");
        }

        if (upstream.MissingUpstreamCanonicalCodes.Count > 0)
        {
            steps.Add($"补齐上游试验项: {string.Join(", ", upstream.MissingUpstreamCanonicalCodes)}");
        }

        if (payloadCoverage.MissingRequiredPayloadFields.Count > 0)
        {
            steps.Add($"补齐 payload 字段: {FormatPreview(payloadCoverage.MissingRequiredPayloadFields, 4)}");
        }

        if (ratedCoverage.MissingRequiredRatedParamFields.Count > 0)
        {
            steps.Add($"补齐额定参数字段: {FormatPreview(ratedCoverage.MissingRequiredRatedParamFields, 4)}");
        }

        if (resultCoverage.MissingRequiredResultFields.Count > 0)
        {
            steps.Add($"补齐结果字段: {FormatPreview(resultCoverage.MissingRequiredResultFields, 4)}");
        }

        if (rawDataCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐原始采样信号: {FormatPreview(rawDataCoverage.MissingSignals, 4)}");
        }

        if (structuredPayloadCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐结构化 payload 信号: {FormatPreview(structuredPayloadCoverage.MissingSignals, 4)}");
        }

        if (structuredResultCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐结构化结果信号: {FormatPreview(structuredResultCoverage.MissingSignals, 4)}");
        }

        if (steps.Count == 0)
        {
            steps.Add($"{canonicalCode} 已具备旧算法适配输入，可进入 adapter 迁移/结果校对");
        }

        return steps.Take(4).ToArray();
    }

    private static IReadOnlyList<MotorYRequiredResultPrimaryFieldDistributionContract> BuildRequiredResultPrimaryFieldDistributions(
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage)
    {
        var candidates = new[]
        {
            (BucketKey: "result-fields", DisplayName: "结果字段", MissingFields: resultCoverage.MissingRequiredResultFields),
            (BucketKey: "intermediate-result-fields", DisplayName: "中间结果锚点", MissingFields: intermediateResultCoverage.MissingRequiredResultFields)
        };

        var groups = candidates
            .SelectMany(bucket => bucket.MissingFields.Select(field => new { bucket.BucketKey, bucket.DisplayName, PrimaryField = field }))
            .GroupBy(x => x.PrimaryField, StringComparer.Ordinal)
            .Select(group =>
            {
                var items = group.ToArray();
                var bucketKeys = items.Select(x => x.BucketKey).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var displayNames = items.Select(x => x.DisplayName).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                return new MotorYRequiredResultPrimaryFieldDistributionContract
                {
                    PrimaryField = group.Key,
                    Count = items.Length,
                    Share = 0d,
                    BucketKeys = bucketKeys,
                    DisplayNames = displayNames,
                    Summary = string.Empty
                };
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.PrimaryField, StringComparer.Ordinal)
            .ToArray();

        if (groups.Length == 0)
        {
            return groups;
        }

        var totalCount = groups.Sum(x => x.Count);
        return groups
            .Select(x =>
            {
                var share = totalCount == 0
                    ? 0d
                    : Math.Round((double)x.Count / totalCount, 4, MidpointRounding.AwayFromZero);
                return new MotorYRequiredResultPrimaryFieldDistributionContract
                {
                    PrimaryField = x.PrimaryField,
                    Count = x.Count,
                    Share = share,
                    BucketKeys = x.BucketKeys,
                    DisplayNames = x.DisplayNames,
                    Summary = $"required-result primary field {x.PrimaryField} missing in {x.Count}/{totalCount} result buckets ({(int)Math.Round(share * 100d, MidpointRounding.AwayFromZero)}pp); buckets={(x.BucketKeys.Count == 0 ? "none" : string.Join(", ", x.BucketKeys))}; displays={(x.DisplayNames.Count == 0 ? "none" : string.Join(", ", x.DisplayNames))}"
                };
            })
            .ToArray();
    }

    private static string BuildRequiredResultPrimaryFieldSummary(IReadOnlyList<MotorYRequiredResultPrimaryFieldDistributionContract> distributions)
    {
        if (distributions.Count == 0)
        {
            return "required-result primary fields: none";
        }

        var preview = distributions
            .Take(3)
            .Select(x => $"{x.PrimaryField}:{x.Count}:{string.Join("/", x.BucketKeys)}")
            .ToArray();

        return $"required-result primary fields: {string.Join(", ", preview)}";
    }

    private static string FormatPreview(IEnumerable<string> values, int maxCount)
    {
        var items = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Take(maxCount + 1)
            .ToArray();

        if (items.Length == 0)
        {
            return "none";
        }

        if (items.Length <= maxCount)
        {
            return string.Join(", ", items);
        }

        return string.Join(", ", items.Take(maxCount)) + ", ...";
    }
}
