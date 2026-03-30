using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.AppSide;

public sealed class TestRecordQueryGatewayAdapter : ITestRecordQueryGateway
{
    private readonly TestRecordQueryFacade _queryFacade;

    public TestRecordQueryGatewayAdapter(TestRecordQueryFacade queryFacade)
    {
        _queryFacade = queryFacade;
    }

    public async Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var views = await _queryFacade.ListRecentForAppAsync(take, cancellationToken);
        return views.Select(MapListItem).ToArray();
    }

    public async Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        var view = await _queryFacade.GetDetailForAppAsync(recordCode, cancellationToken);
        return view is null ? null : MapDetail(view);
    }

    private static TestRecordListItemContract MapListItem(TestRecordListView view)
    {
        return new TestRecordListItemContract
        {
            RecordCode = view.RecordCode,
            ProductKind = view.ProductKind,
            ProductDisplayName = view.ProductDisplayName,
            TestKindCode = view.TestKindCode,
            TestTime = view.TestTime,
            ItemCount = view.ItemCount,
            SampleCount = view.SampleCount,
            KeyPointSampleCount = view.KeyPointSampleCount,
            ContinuousSampleCount = view.ContinuousSampleCount,
            RecordAttachmentCount = view.RecordAttachmentCount,
            ItemAttachmentBucketCount = view.ItemAttachmentBucketCount,
            ReportCount = view.ReportCount,
            HasReportArtifacts = view.HasReportArtifacts,
            ReusedProductDefinition = view.ReusedProductDefinition,
            PrimaryReportFormat = view.PrimaryReportFormat,
            PrimaryReportArtifactFileName = view.PrimaryReportArtifactFileName,
            LightweightReportFormat = view.LightweightReportFormat,
            LightweightReportArtifactFileName = view.LightweightReportArtifactFileName,
            ItemPartitions = view.ItemPartitions
        };
    }

    private static TestRecordDetailContract MapDetail(TestRecordDetailView view)
    {
        return new TestRecordDetailContract
        {
            RecordCode = view.RecordCode,
            ProductKind = view.ProductKind,
            ProductDisplayName = view.ProductDisplayName,
            TestKindCode = view.TestKindCode,
            TestTime = view.TestTime,
            RecordAttachmentCount = view.RecordAttachmentCount,
            ItemAttachmentBucketCount = view.ItemAttachmentBucketCount,
            ItemCount = view.ItemCount,
            SampleCount = view.SampleCount,
            KeyPointSampleCount = view.KeyPointSampleCount,
            ContinuousSampleCount = view.ContinuousSampleCount,
            HasReports = view.HasReports,
            HasReportArtifacts = view.HasReportArtifacts,
            PrimaryReportFormat = view.PrimaryReportFormat,
            PrimaryReportArtifactFileName = view.PrimaryReportArtifactFileName,
            LightweightReportFormat = view.LightweightReportFormat,
            LightweightReportArtifactFileName = view.LightweightReportArtifactFileName,
            ItemDetails = view.ItemDetails.Select(item => new TestRecordItemDetailContract
            {
                ItemCode = item.ItemCode,
                DisplayName = item.DisplayName,
                SortOrder = item.SortOrder,
                MethodCode = item.MethodCode,
                RecordMode = item.RecordMode ?? string.Empty,
                SampleCount = item.SampleCount,
                LegacySampleCount = item.LegacySampleCount,
                HasLegacyPayload = item.HasLegacyPayload,
                LegacyPayload = new TestRecordLegacyPayloadContract
                {
                    LegacySampleCount = item.LegacyPayload.LegacySampleCount,
                    HasLegacyPayload = item.LegacyPayload.HasLegacyPayload,
                    PowerCurveImageCount = item.LegacyPayload.PowerCurveImageCount,
                    TempCurveImageCount = item.LegacyPayload.TempCurveImageCount,
                    VibrationCurveImageCount = item.LegacyPayload.VibrationCurveImageCount,
                    HasIncomingPowerMetrics = item.LegacyPayload.HasIncomingPowerMetrics,
                    HasWindingTemperatureMetrics = item.LegacyPayload.HasWindingTemperatureMetrics
                },
                BuildProfile = item.BuildProfile is null
                    ? null
                    : new MotorYBuildProfileContract
                    {
                        CanonicalCode = item.BuildProfile.CanonicalCode,
                        MethodValue = item.BuildProfile.MethodValue,
                        MethodKey = item.BuildProfile.MethodKey,
                        ProfileKey = item.BuildProfile.ProfileKey,
                        VariantKind = item.BuildProfile.VariantKind,
                        AlgorithmFamily = item.BuildProfile.AlgorithmFamily,
                        LegacyEnumName = item.BuildProfile.LegacyEnumName,
                        LegacyFormName = item.BuildProfile.LegacyFormName,
                        LegacyAlgorithmEntry = item.BuildProfile.LegacyAlgorithmEntry,
                        LegacyMethodName = item.BuildProfile.LegacyMethodName,
                        LegacySettingsMethodName = item.BuildProfile.LegacySettingsMethodName,
                        IsBaselineMethod = item.BuildProfile.IsBaselineMethod
                    },
                LegacyAlgorithmRoute = MapBuildProfile(item.LegacyAlgorithmRoute),
                AttachmentCount = item.AttachmentCount,
                IsValid = item.IsValid,
                HasRemark = item.HasRemark,
                Remark = item.Remark ?? string.Empty
            }).ToArray(),
            MotorYMethodDecisions = view.MotorYMethodDecisions.Select(MapMotorYMethodDecision).ToArray(),
            MotorYMethodAdaptationPlans = view.MotorYMethodAdaptationPlans
                .Select(MapMotorYMethodAdaptationPlan)
                .ToArray(),
            ReportSummaries = view.ReportSummaries.Select(summary => new TestReportSummaryContract
            {
                RecordCode = summary.RecordCode,
                Format = summary.Format,
                ExportedAt = summary.ExportedAt,
                ContentLength = summary.ContentLength,
                ArtifactFileName = summary.ArtifactFileName,
                ArtifactSavedPath = summary.ArtifactSavedPath,
                IsLightweightEntry = summary.IsLightweightEntry,
                IsPrimaryEntry = summary.IsPrimaryEntry
            }).ToArray()
        };
    }

    private static MotorYMethodDecisionContract MapMotorYMethodDecision(MotorYMethodDecisionSnapshot snapshot)
    {
        var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(snapshot);

        return new MotorYMethodDecisionContract
        {
            CanonicalCode = snapshot.CanonicalCode,
            TotalCount = snapshot.TotalCount,
            BaselineProfile = MapBuildProfile(snapshot.BaselineRoute),
            BaselineCount = snapshot.BaselineCount,
            DominantProfile = MapBuildProfile(snapshot.DominantRoute),
            DominantCount = snapshot.DominantCount,
            RecommendedProfile = MapBuildProfile(snapshot.RecommendedRoute),
            RecommendedStrategy = snapshot.RecommendedStrategy,
            ShouldPrioritizeDominantOverBaseline = snapshot.ShouldPrioritizeDominantOverBaseline,
            DominantShare = snapshot.DominantShare,
            BaselineShare = snapshot.BaselineShare,
            DominantOverrideThreshold = snapshot.DominantOverrideThreshold,
            DominantLeadCount = snapshot.DominantLeadCount,
            DominantLeadPercentagePoints = snapshot.DominantLeadPercentagePoints,
            RecommendationReason = snapshot.RecommendationReason,
            RecommendedMethodSummary = selection.SelectedMethodSummary,
            BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
            Distributions = snapshot.Distributions.Select(MapMotorYMethodDistribution).ToArray()
        };
    }

    private static MotorYMethodDistributionContract MapMotorYMethodDistribution(MotorYMethodDistributionSnapshot snapshot)
    {
        return new MotorYMethodDistributionContract
        {
            MethodValue = snapshot.MethodValue,
            Count = snapshot.Count,
            Share = snapshot.Share,
            Profile = MapBuildProfile(snapshot.Route)
        };
    }

    private static MotorYMethodAdaptationPlanContract MapMotorYMethodAdaptationPlan(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        return new MotorYMethodAdaptationPlanContract
        {
            CanonicalCode = snapshot.CanonicalCode,
            DecisionAnchorTopPriority = snapshot.DecisionAnchorTopPriority,
            DecisionAnchorTopPrioritySummary = snapshot.DecisionAnchorTopPrioritySummary,
            DecisionAnchorTopPriorityDominantAnchorKey = snapshot.DecisionAnchorTopPriorityDominantAnchorKey,
            DecisionAnchorTopPriorityFocus = snapshot.DecisionAnchorTopPriorityFocus,
            DecisionAnchorTopPriorityFields = snapshot.DecisionAnchorTopPriorityFields,
            DecisionAnchorTopPriorityNextStepSummary = snapshot.DecisionAnchorTopPriorityNextStepSummary,
            DecisionAnchorTopPriorityPrimaryField = snapshot.DecisionAnchorTopPriorityPrimaryField,
            DecisionAnchorTopPriorityPrimaryFieldSummary = snapshot.DecisionAnchorTopPriorityPrimaryFieldSummary,
            DecisionAnchorTopPriorityDetail = snapshot.DecisionAnchorTopPriorityDetail is null
                ? null
                : new MotorYDecisionAnchorTopPriorityContract
                {
                    Priority = snapshot.DecisionAnchorTopPriorityDetail.Priority,
                    AnchorKey = snapshot.DecisionAnchorTopPriorityDetail.AnchorKey,
                    Focus = snapshot.DecisionAnchorTopPriorityDetail.Focus,
                    Fields = snapshot.DecisionAnchorTopPriorityDetail.Fields,
                    NextStepSummary = snapshot.DecisionAnchorTopPriorityDetail.NextStepSummary,
                    PrimaryField = snapshot.DecisionAnchorTopPriorityDetail.PrimaryField,
                    PrimaryFieldSummary = snapshot.DecisionAnchorTopPriorityDetail.PrimaryFieldSummary,
                    Summary = snapshot.DecisionAnchorTopPriorityDetail.Summary
                },
            TotalCount = snapshot.TotalCount,
            BaselineProfile = MapBuildProfile(snapshot.BaselineRoute),
            BaselineCount = snapshot.BaselineCount,
            BaselineShare = snapshot.BaselineShare,
            DominantProfile = MapBuildProfile(snapshot.DominantRoute),
            DominantCount = snapshot.DominantCount,
            DominantShare = snapshot.DominantShare,
            SelectedProfile = MapBuildProfile(snapshot.SelectedRoute),
            SelectedCount = snapshot.SelectedCount,
            SelectedShare = snapshot.SelectedShare,
            SelectionStrategy = snapshot.SelectionStrategy,
            ShouldUseDominantRoute = snapshot.ShouldUseDominantRoute,
            DominantOverrideThreshold = snapshot.DominantOverrideThreshold,
            DominantLeadCount = snapshot.DominantLeadCount,
            DominantLeadPercentagePoints = snapshot.DominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = snapshot.SelectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = snapshot.SelectedLeadPercentagePointsVsBaseline,
            SelectionReason = snapshot.SelectionReason,
            AlgorithmFamily = snapshot.AlgorithmFamily,
            AlgorithmEntry = snapshot.AlgorithmEntry,
            SettingsMethodName = snapshot.SettingsMethodName,
            LegacyMethodName = snapshot.LegacyMethodName,
            RecommendedLegacyCode = snapshot.RecommendedLegacyCode,
            DominantLegacyCode = snapshot.DominantLegacyCode,
            RecommendedLegacyCodeCount = snapshot.RecommendedLegacyCodeCount,
            RecommendedLegacyCodeShare = snapshot.RecommendedLegacyCodeShare,
            LegacyCodeSelectionSummary = snapshot.LegacyCodeSelectionSummary,
            LegacyCodeDistributions = snapshot.LegacyCodeDistributions.Select(x => new MotorYLegacyCodeDistributionContract
            {
                CanonicalCode = x.CanonicalCode,
                LegacyCode = x.LegacyCode,
                Count = x.Count,
                Share = x.Share
            }).ToArray(),
            RequiresRatedParams = snapshot.RequiresRatedParams,
            UpstreamCanonicalCodes = snapshot.UpstreamCanonicalCodes,
            UpstreamLegacyAliases = snapshot.UpstreamLegacyAliases,
            UpstreamLegacyCodeDistributions = snapshot.UpstreamLegacyCodeDistributions.Select(x => new MotorYLegacyUpstreamCodeDistributionContract
            {
                CanonicalCode = x.CanonicalCode,
                LegacyCode = x.LegacyCode,
                Count = x.Count,
                Share = x.Share
            }).ToArray(),
            ObservedUpstreamCanonicalCodeCount = snapshot.ObservedUpstreamCanonicalCodeCount,
            ObservedUpstreamCanonicalCodes = snapshot.ObservedUpstreamCanonicalCodes,
            ObservedUpstreamLegacyCodes = snapshot.ObservedUpstreamLegacyCodes,
            MissingUpstreamCanonicalCodes = snapshot.MissingUpstreamCanonicalCodes,
            UpstreamDependenciesSatisfied = snapshot.UpstreamDependenciesSatisfied,
            UpstreamDependencySummary = snapshot.UpstreamDependencySummary,
            RequiredPayloadFields = snapshot.RequiredPayloadFields,
            SourceEvidences = snapshot.SourceEvidences.Select(x => new MotorYLegacyAlgorithmSourceEvidenceContract
            {
                SectionKey = x.SectionKey,
                MethodName = x.MethodName,
                SourceFile = x.SourceFile,
                StartLine = x.StartLine,
                EndLine = x.EndLine,
                SourceRange = x.SourceRange,
                SourceAnchor = x.SourceAnchor,
                ReferencedFields = x.ReferencedFields,
                Summary = x.Summary
            }).ToArray(),
            RequiredRatedParamFields = snapshot.RequiredRatedParamFields,
            RequiredResultFields = snapshot.RequiredResultFields,
            RequiredIntermediateResultFields = snapshot.RequiredIntermediateResultFields,
            CoveredRequiredIntermediateResultFieldCount = snapshot.CoveredRequiredIntermediateResultFieldCount,
            MissingRequiredIntermediateResultFieldCount = snapshot.MissingRequiredIntermediateResultFieldCount,
            MissingRequiredIntermediateResultFields = snapshot.MissingRequiredIntermediateResultFields,
            CoveredRequiredIntermediateResultFields = snapshot.CoveredRequiredIntermediateResultFields,
            RequiredIntermediateResultFieldCoverageRatio = snapshot.RequiredIntermediateResultFieldCoverageRatio,
            RequiredIntermediateResultFieldCoveragePercentagePoints = snapshot.RequiredIntermediateResultFieldCoveragePercentagePoints,
            RequiredIntermediateResultFieldCoverageSummary = snapshot.RequiredIntermediateResultFieldCoverageSummary,
            CoveredRequiredResultFieldCount = snapshot.CoveredRequiredResultFieldCount,
            MissingRequiredResultFieldCount = snapshot.MissingRequiredResultFieldCount,
            MissingRequiredResultFields = snapshot.MissingRequiredResultFields,
            CoveredRequiredResultFields = snapshot.CoveredRequiredResultFields,
            RequiredResultFieldCoverageRatio = snapshot.RequiredResultFieldCoverageRatio,
            RequiredResultFieldCoveragePercentagePoints = snapshot.RequiredResultFieldCoveragePercentagePoints,
            RequiredResultFieldCoverageSummary = snapshot.RequiredResultFieldCoverageSummary,
            CoveredRequiredPayloadFieldCount = snapshot.CoveredRequiredPayloadFieldCount,
            MissingRequiredPayloadFieldCount = snapshot.MissingRequiredPayloadFieldCount,
            MissingRequiredPayloadFields = snapshot.MissingRequiredPayloadFields,
            CoveredRequiredPayloadFields = snapshot.CoveredRequiredPayloadFields,
            RequiredPayloadFieldCoverageRatio = snapshot.RequiredPayloadFieldCoverageRatio,
            RequiredPayloadFieldCoveragePercentagePoints = snapshot.RequiredPayloadFieldCoveragePercentagePoints,
            SamplePayloadAvailable = snapshot.SamplePayloadAvailable,
            RequiredPayloadFieldCoverageSummary = snapshot.RequiredPayloadFieldCoverageSummary,
            RequiredRawDataSignals = snapshot.RequiredRawDataSignals,
            ObservedRawDataSignals = snapshot.ObservedRawDataSignals,
            MissingRawDataSignals = snapshot.MissingRawDataSignals,
            RawDataSignalCoveredCount = snapshot.RawDataSignalCoveredCount,
            RawDataSignalMissingCount = snapshot.RawDataSignalMissingCount,
            RawDataSampleCount = snapshot.RawDataSampleCount,
            RawDataListAvailable = snapshot.RawDataListAvailable,
            RawDataSignalCoverageRatio = snapshot.RawDataSignalCoverageRatio,
            RawDataSignalCoveragePercentagePoints = snapshot.RawDataSignalCoveragePercentagePoints,
            RawDataSignalCoverageSummary = snapshot.RawDataSignalCoverageSummary,
            CoveredRequiredRatedParamFieldCount = snapshot.CoveredRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFieldCount = snapshot.MissingRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFields = snapshot.MissingRequiredRatedParamFields,
            CoveredRequiredRatedParamFields = snapshot.CoveredRequiredRatedParamFields,
            RequiredRatedParamFieldCoverageRatio = snapshot.RequiredRatedParamFieldCoverageRatio,
            RequiredRatedParamFieldCoveragePercentagePoints = snapshot.RequiredRatedParamFieldCoveragePercentagePoints,
            RatedParamsAvailable = snapshot.RatedParamsAvailable,
            RequiredRatedParamFieldCoverageSummary = snapshot.RequiredRatedParamFieldCoverageSummary,
            LegacyAlgorithmInputsReady = snapshot.LegacyAlgorithmInputsReady,
            ObservedAlgorithmInputFields = snapshot.ObservedAlgorithmInputFields,
            ObservedAlgorithmInputFieldSources = snapshot.ObservedAlgorithmInputFieldSources.Select(x => new MotorYObservedFieldSourceContract
            {
                FieldName = x.FieldName,
                SourceType = x.SourceType,
                SourceScope = x.SourceScope,
                SourceSummary = x.SourceSummary
            }).ToArray(),
            MissingAlgorithmInputFields = snapshot.MissingAlgorithmInputFields,
            ObservedAlgorithmInputFieldCount = snapshot.ObservedAlgorithmInputFieldCount,
            MissingAlgorithmInputFieldCount = snapshot.MissingAlgorithmInputFieldCount,
            AlgorithmInputFieldCoverageRatio = snapshot.AlgorithmInputFieldCoverageRatio,
            AlgorithmInputFieldCoveragePercentagePoints = snapshot.AlgorithmInputFieldCoveragePercentagePoints,
            AlgorithmInputFieldCoverageSummary = snapshot.AlgorithmInputFieldCoverageSummary,
            RawDataSignalsReady = snapshot.RawDataSignalsReady,
            MinimumRawSampleCount = snapshot.MinimumRawSampleCount,
            RawSampleCountReady = snapshot.RawSampleCountReady,
            RawSampleCountReadinessSummary = snapshot.RawSampleCountReadinessSummary,
            RawSampleCountGap = snapshot.RawSampleCountGap,
            RawSampleCountDecisionSummary = snapshot.RawSampleCountDecisionSummary,
            RequiredStructuredPayloadSignals = snapshot.RequiredStructuredPayloadSignals,
            MinimumStructuredPayloadSampleCount = snapshot.MinimumStructuredPayloadSampleCount,
            StructuredPayloadSampleCountReady = snapshot.StructuredPayloadSampleCountReady,
            StructuredPayloadSampleCountReadinessSummary = snapshot.StructuredPayloadSampleCountReadinessSummary,
            StructuredPayloadSampleCountGap = snapshot.StructuredPayloadSampleCountGap,
            StructuredPayloadSampleCountDecisionSummary = snapshot.StructuredPayloadSampleCountDecisionSummary,
            ObservedStructuredPayloadSignals = snapshot.ObservedStructuredPayloadSignals,
            MissingStructuredPayloadSignals = snapshot.MissingStructuredPayloadSignals,
            StructuredPayloadSignalCoveredCount = snapshot.StructuredPayloadSignalCoveredCount,
            StructuredPayloadSignalMissingCount = snapshot.StructuredPayloadSignalMissingCount,
            StructuredPayloadSampleCount = snapshot.StructuredPayloadSampleCount,
            StructuredPayloadAvailable = snapshot.StructuredPayloadAvailable,
            StructuredPayloadSignalCoverageRatio = snapshot.StructuredPayloadSignalCoverageRatio,
            StructuredPayloadSignalCoveragePercentagePoints = snapshot.StructuredPayloadSignalCoveragePercentagePoints,
            StructuredPayloadSignalCoverageSummary = snapshot.StructuredPayloadSignalCoverageSummary,
            RequiredStructuredResultSignals = snapshot.RequiredStructuredResultSignals,
            MinimumStructuredResultSampleCount = snapshot.MinimumStructuredResultSampleCount,
            StructuredResultSampleCountReady = snapshot.StructuredResultSampleCountReady,
            StructuredResultSampleCountReadinessSummary = snapshot.StructuredResultSampleCountReadinessSummary,
            StructuredResultSampleCountGap = snapshot.StructuredResultSampleCountGap,
            StructuredResultSampleCountDecisionSummary = snapshot.StructuredResultSampleCountDecisionSummary,
            ObservedStructuredResultSignals = snapshot.ObservedStructuredResultSignals,
            MissingStructuredResultSignals = snapshot.MissingStructuredResultSignals,
            StructuredResultSignalCoveredCount = snapshot.StructuredResultSignalCoveredCount,
            StructuredResultSignalMissingCount = snapshot.StructuredResultSignalMissingCount,
            StructuredResultSampleCount = snapshot.StructuredResultSampleCount,
            StructuredResultAvailable = snapshot.StructuredResultAvailable,
            StructuredResultSignalCoverageRatio = snapshot.StructuredResultSignalCoverageRatio,
            StructuredResultSignalCoveragePercentagePoints = snapshot.StructuredResultSignalCoveragePercentagePoints,
            StructuredResultSignalCoverageSummary = snapshot.StructuredResultSignalCoverageSummary,
            LegacyAlgorithmInputReadinessSummary = snapshot.LegacyAlgorithmInputReadinessSummary,
            DependencyNotes = snapshot.DependencyNotes,
            SuggestedNextSteps = snapshot.SuggestedNextSteps,
            SuggestedNextStepSummary = snapshot.SuggestedNextStepSummary,
            FormulaSignals = snapshot.FormulaSignals,
            CoveredFormulaSignalCount = snapshot.CoveredFormulaSignalCount,
            MissingFormulaSignalCount = snapshot.MissingFormulaSignalCount,
            CoveredFormulaSignals = snapshot.CoveredFormulaSignals,
            MissingFormulaSignals = snapshot.MissingFormulaSignals,
            FormulaSignalCoverageRatio = snapshot.FormulaSignalCoverageRatio,
            FormulaSignalCoveragePercentagePoints = snapshot.FormulaSignalCoveragePercentagePoints,
            FormulaSignalsBackedByObservedPayload = snapshot.FormulaSignalsBackedByObservedPayload,
            FormulaSignalsObservedPayloadFields = snapshot.FormulaSignalsObservedPayloadFields,
            FormulaSignalObservedPayloadGaps = snapshot.FormulaSignalObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapContract
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            FormulaSignalsObservedPayloadSummary = snapshot.FormulaSignalsObservedPayloadSummary,
            LegacyAlgorithmRules = snapshot.LegacyAlgorithmRules,
            CoveredLegacyAlgorithmRuleCount = snapshot.CoveredLegacyAlgorithmRuleCount,
            MissingLegacyAlgorithmRuleCount = snapshot.MissingLegacyAlgorithmRuleCount,
            CoveredLegacyAlgorithmRules = snapshot.CoveredLegacyAlgorithmRules,
            MissingLegacyAlgorithmRules = snapshot.MissingLegacyAlgorithmRules,
            LegacyAlgorithmRuleCoverageRatio = snapshot.LegacyAlgorithmRuleCoverageRatio,
            LegacyAlgorithmRuleCoveragePercentagePoints = snapshot.LegacyAlgorithmRuleCoveragePercentagePoints,
            LegacyAlgorithmRulesBackedByObservedPayload = snapshot.LegacyAlgorithmRulesBackedByObservedPayload,
            LegacyAlgorithmRulesObservedPayloadFields = snapshot.LegacyAlgorithmRulesObservedPayloadFields,
            LegacyAlgorithmRulesObservedPayloadGaps = snapshot.LegacyAlgorithmRulesObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapContract
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            LegacyAlgorithmRulesObservedPayloadSummary = snapshot.LegacyAlgorithmRulesObservedPayloadSummary,
            LegacyDecisionAnchors = snapshot.LegacyDecisionAnchors,
            CoveredLegacyDecisionAnchorCount = snapshot.CoveredLegacyDecisionAnchorCount,
            MissingLegacyDecisionAnchorCount = snapshot.MissingLegacyDecisionAnchorCount,
            CoveredLegacyDecisionAnchors = snapshot.CoveredLegacyDecisionAnchors,
            MissingLegacyDecisionAnchors = snapshot.MissingLegacyDecisionAnchors,
            LegacyDecisionAnchorCoverageRatio = snapshot.LegacyDecisionAnchorCoverageRatio,
            LegacyDecisionAnchorCoveragePercentagePoints = snapshot.LegacyDecisionAnchorCoveragePercentagePoints,
            LegacyDecisionAnchorsBackedByObservedPayload = snapshot.LegacyDecisionAnchorsBackedByObservedPayload,
            LegacyDecisionAnchorsObservedPayloadFields = snapshot.LegacyDecisionAnchorsObservedPayloadFields,
            LegacyDecisionAnchorsObservedPayloadGaps = snapshot.LegacyDecisionAnchorsObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapContract
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            LegacyDecisionAnchorObservationRules = snapshot.LegacyDecisionAnchorObservationRules.Select(MapDecisionAnchorObservationRule).ToArray(),
            LegacyDecisionAnchorResolutions = snapshot.LegacyDecisionAnchorResolutions.Select(MapDecisionAnchorResolution).ToArray(),
            CoveredLegacyDecisionAnchorObservationRuleCount = snapshot.CoveredLegacyDecisionAnchorObservationRuleCount,
            MissingLegacyDecisionAnchorObservationRuleCount = snapshot.MissingLegacyDecisionAnchorObservationRuleCount,
            ResolvedLegacyDecisionAnchorCount = snapshot.ResolvedLegacyDecisionAnchorCount,
            PartialLegacyDecisionAnchorCount = snapshot.PartialLegacyDecisionAnchorCount,
            MissingLegacyDecisionAnchorResolutionCount = snapshot.MissingLegacyDecisionAnchorResolutionCount,
            LegacyDecisionAnchorObservationRuleCoverageRatio = snapshot.LegacyDecisionAnchorObservationRuleCoverageRatio,
            LegacyDecisionAnchorObservationRuleCoveragePercentagePoints = snapshot.LegacyDecisionAnchorObservationRuleCoveragePercentagePoints,
            LegacyDecisionAnchorResolutionCoverageRatio = snapshot.LegacyDecisionAnchorResolutionCoverageRatio,
            LegacyDecisionAnchorResolutionCoveragePercentagePoints = snapshot.LegacyDecisionAnchorResolutionCoveragePercentagePoints,
            LegacyDecisionAnchorObservationRuleSummary = snapshot.LegacyDecisionAnchorObservationRuleSummary,
            LegacyDecisionAnchorResolutionSummary = snapshot.LegacyDecisionAnchorResolutionSummary,
            LegacyDecisionAnchorNextActionSummary = snapshot.LegacyDecisionAnchorNextActionSummary,
            LegacyDecisionAnchorGapPreviewSummary = snapshot.LegacyDecisionAnchorGapPreviewSummary,
            SuggestedDecisionAnchorNextSteps = snapshot.SuggestedDecisionAnchorNextSteps,
            SuggestedDecisionAnchorNextStepSummary = snapshot.SuggestedDecisionAnchorNextStepSummary,
            DecisionAnchorPriorityDistributions = snapshot.DecisionAnchorPriorityDistributions.Select(x => new MotorYDecisionAnchorPriorityDistributionContract
            {
                Priority = x.Priority,
                Count = x.Count,
                Share = x.Share,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepFields = x.SuggestedNextStepFields,
                SuggestedNextSteps = x.SuggestedNextSteps,
                SuggestedNextStepSummary = x.SuggestedNextStepSummary,
                DominantAnchorKey = x.DominantAnchorKey,
                DominantSuggestedNextStepFocus = x.DominantSuggestedNextStepFocus,
                DominantSuggestedNextStepFields = x.DominantSuggestedNextStepFields,
                DominantSuggestedNextStepSummary = x.DominantSuggestedNextStepSummary
            }).ToArray(),
            DecisionAnchorPrimaryFieldDistributions = snapshot.DecisionAnchorPrimaryFieldDistributions.Select(x => new MotorYDecisionAnchorPrimaryFieldDistributionContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                CanonicalCodes = x.CanonicalCodes,
                Summary = x.Summary
            }).ToArray(),
            CrossPlanDecisionAnchorPrimaryFieldFocuses = snapshot.CrossPlanDecisionAnchorPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            CrossPlanDecisionAnchorPrimaryFieldSummary = snapshot.CrossPlanDecisionAnchorPrimaryFieldSummary,
            AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses = snapshot.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            AlgorithmFamilyDecisionAnchorPrimaryFieldSummary = snapshot.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary,
            VariantKindDecisionAnchorPrimaryFieldFocuses = snapshot.VariantKindDecisionAnchorPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            VariantKindDecisionAnchorPrimaryFieldSummary = snapshot.VariantKindDecisionAnchorPrimaryFieldSummary,
            RequiredResultPrimaryFieldDistributions = snapshot.RequiredResultPrimaryFieldDistributions.Select(x => new MotorYRequiredResultPrimaryFieldDistributionContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                BucketKeys = x.BucketKeys,
                DisplayNames = x.DisplayNames,
                Summary = x.Summary
            }).ToArray(),
            RequiredResultPrimaryFieldSummary = snapshot.RequiredResultPrimaryFieldSummary,
            CrossPlanRequiredResultPrimaryFieldFocuses = snapshot.CrossPlanRequiredResultPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            CrossPlanRequiredResultPrimaryFieldSummary = snapshot.CrossPlanRequiredResultPrimaryFieldSummary,
            AlgorithmFamilyRequiredResultPrimaryFieldFocuses = snapshot.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            AlgorithmFamilyRequiredResultPrimaryFieldSummary = snapshot.AlgorithmFamilyRequiredResultPrimaryFieldSummary,
            VariantKindRequiredResultPrimaryFieldFocuses = snapshot.VariantKindRequiredResultPrimaryFieldFocuses.Select(x => new MotorYPrimaryFieldFocusContract
            {
                PrimaryField = x.PrimaryField,
                Count = x.Count,
                Share = x.Share,
                WeightedCount = x.WeightedCount,
                WeightedShare = x.WeightedShare,
                CanonicalCodes = x.CanonicalCodes,
                AlgorithmFamilies = x.AlgorithmFamilies,
                VariantKinds = x.VariantKinds,
                AnchorKeys = x.AnchorKeys,
                SuggestedNextStepFocuses = x.SuggestedNextStepFocuses,
                SuggestedNextStepPriorities = x.SuggestedNextStepPriorities,
                Summary = x.Summary
            }).ToArray(),
            VariantKindRequiredResultPrimaryFieldSummary = snapshot.VariantKindRequiredResultPrimaryFieldSummary,
            DecisionAnchorPrioritySummary = snapshot.DecisionAnchorPrioritySummary,
            EffectiveLegacyDecisionAnchorCoverageCount = snapshot.EffectiveLegacyDecisionAnchorCoverageCount,
            EffectiveLegacyDecisionAnchorGapCount = snapshot.EffectiveLegacyDecisionAnchorGapCount,
            EffectiveLegacyDecisionAnchorCoverageRatio = snapshot.EffectiveLegacyDecisionAnchorCoverageRatio,
            EffectiveLegacyDecisionAnchorCoveragePercentagePoints = snapshot.EffectiveLegacyDecisionAnchorCoveragePercentagePoints,
            LegacyDecisionAnchorReady = snapshot.LegacyDecisionAnchorReady,
            LegacyDecisionAnchorsObservedPayloadSummary = snapshot.LegacyDecisionAnchorsObservedPayloadSummary,
            FormulaSignalSummary = snapshot.FormulaSignalSummary,
            LegacyAlgorithmRuleSummary = snapshot.LegacyAlgorithmRuleSummary,
            LegacyDecisionAnchorSummary = snapshot.LegacyDecisionAnchorSummary,
            SelectedMethodSummary = snapshot.SelectedMethodSummary,
            BaselineDominantComparisonSummary = snapshot.BaselineDominantComparisonSummary,
            DependencyBuckets = snapshot.DependencyBuckets.Select(x => new MotorYDependencyBucketSummaryContract
            {
                BucketKey = x.BucketKey,
                DisplayName = x.DisplayName,
                RequiredCount = x.RequiredCount,
                CoveredCount = x.CoveredCount,
                MissingCount = x.MissingCount,
                CoverageRatio = x.CoverageRatio,
                CoveragePercentagePoints = x.CoveragePercentagePoints,
                RequiredItems = x.RequiredItems,
                CoveredItems = x.CoveredItems,
                MissingItems = x.MissingItems,
                Summary = x.Summary
            }).ToArray(),
            Distributions = snapshot.Distributions.Select(MapMotorYMethodDistribution).ToArray()
        };
    }

    private static MotorYDecisionAnchorObservationRuleContract MapDecisionAnchorObservationRule(MotorYDecisionAnchorObservationRuleSnapshot snapshot)
    {
        return new MotorYDecisionAnchorObservationRuleContract
        {
            AnchorKey = snapshot.AnchorKey,
            RequiredPayloadFields = snapshot.RequiredPayloadFields,
            ObservedPayloadFields = snapshot.ObservedPayloadFields,
            MissingPayloadFields = snapshot.MissingPayloadFields,
            CoveredByObservedPayload = snapshot.CoveredByObservedPayload,
            Summary = snapshot.Summary
        };
    }

    private static MotorYDecisionAnchorResolutionContract MapDecisionAnchorResolution(MotorYDecisionAnchorResolutionSnapshot snapshot)
    {
        return new MotorYDecisionAnchorResolutionContract
        {
            AnchorKey = snapshot.AnchorKey,
            ResolvedByObservedPayload = snapshot.ResolvedByObservedPayload,
            PartiallyResolvedByObservedPayload = snapshot.PartiallyResolvedByObservedPayload,
            RequiredPayloadFields = snapshot.RequiredPayloadFields,
            ObservedPayloadFields = snapshot.ObservedPayloadFields,
            MissingPayloadFields = snapshot.MissingPayloadFields,
            CoverageRatio = snapshot.CoverageRatio,
            CoveragePercentagePoints = snapshot.CoveragePercentagePoints,
            ResolutionStage = snapshot.ResolutionStage,
            SuggestedNextStepCategory = snapshot.SuggestedNextStepCategory,
            SuggestedNextStepFocus = snapshot.SuggestedNextStepFocus,
            SuggestedNextStepFields = snapshot.SuggestedNextStepFields,
            SuggestedNextSteps = snapshot.SuggestedNextSteps,
            SuggestedNextStepSummary = snapshot.SuggestedNextStepSummary,
            SuggestedNextStepPriority = snapshot.SuggestedNextStepPriority,
            SuggestedNextStepPrioritySummary = snapshot.SuggestedNextStepPrioritySummary,
            SuggestedNextStepCoverageSummary = snapshot.SuggestedNextStepCoverageSummary,
            SuggestedPrimaryNextField = snapshot.SuggestedPrimaryNextField,
            SuggestedPrimaryNextFieldSummary = snapshot.SuggestedPrimaryNextFieldSummary,
            Summary = snapshot.Summary
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
