using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordViewMapper
{
    internal const double MotorYDominantOverrideThreshold = 0.7d;

    public static TestRecordListView ToListView(this TestRecordSummary summary)
    {
        return new TestRecordListView
        {
            RecordCode = summary.RecordCode,
            ProductKind = summary.ProductKind,
            ProductDisplayName = BuildProductDisplayName(summary.ProductModel, summary.ProductCode, summary.ProductKind),
            TestKindCode = summary.TestKindCode,
            TestTime = summary.TestTime,
            ItemCount = summary.ItemCount,
            SampleCount = summary.Mapping.TotalSampleCount,
            KeyPointSampleCount = summary.Mapping.KeyPointSampleCount,
            ContinuousSampleCount = summary.Mapping.ContinuousSampleCount,
            RecordAttachmentCount = summary.RecordAttachmentCount,
            ItemAttachmentBucketCount = summary.ItemAttachmentBucketCount,
            ReportCount = summary.ReportCount,
            HasReportArtifacts = summary.HasReportArtifacts,
            ReusedProductDefinition = summary.ReusedProductDefinition,
            PrimaryReportFormat = summary.PrimaryReportFormat,
            PrimaryReportArtifactFileName = summary.PrimaryReportArtifactFileName,
            LightweightReportFormat = summary.LightweightReportFormat,
            LightweightReportArtifactFileName = summary.LightweightReportArtifactFileName,
            ItemPartitions = summary.Mapping.Partitions
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ItemCode, StringComparer.Ordinal)
                .Select(x => new TestRecordItemPartitionContract
                {
                    ItemCode = x.ItemCode,
                    DisplayName = x.DisplayName,
                    SortOrder = x.SortOrder,
                    MethodCode = x.MethodCode,
                    RecordMode = x.RecordMode,
                    SampleCount = x.SampleCount,
                    LegacySampleCount = x.LegacySampleCount,
                    HasLegacyPayload = x.HasLegacyPayload,
                    HasRemark = !string.IsNullOrWhiteSpace(x.Remark),
                    Remark = x.Remark
                })
                .ToArray()
        };
    }

    public static TestRecordDetailView ToDetailView(this TestRecordDetail detail)
    {
        return new TestRecordDetailView
        {
            RecordCode = detail.Record.RecordCode,
            ProductKind = detail.Record.ProductKind,
            ProductDisplayName = BuildProductDisplayName(detail.Record.TestProduct?.Model, detail.Record.TestProduct?.Code, detail.Record.ProductKind),
            TestKindCode = detail.Record.TestKindCode,
            TestTime = detail.Record.TestTime,
            RecordAttachmentCount = detail.RecordAttachments.Count,
            ItemAttachmentBucketCount = detail.ItemAttachments.Count,
            ItemCount = detail.ItemDetails.Count,
            SampleCount = detail.Mapping.TotalSampleCount,
            KeyPointSampleCount = detail.Mapping.KeyPointSampleCount,
            ContinuousSampleCount = detail.Mapping.ContinuousSampleCount,
            HasReports = detail.HasReports,
            HasReportArtifacts = detail.HasReportArtifacts,
            PrimaryReportFormat = detail.PrimaryReport?.Format,
            PrimaryReportArtifactFileName = detail.PrimaryReport?.ArtifactFileName,
            LightweightReportFormat = detail.LightweightReport?.Format,
            LightweightReportArtifactFileName = detail.LightweightReport?.ArtifactFileName,
            ItemDetails = detail.ItemDetails,
            MotorYMethodDecisions = BuildMotorYMethodDecisions(detail.ItemDetails),
            MotorYMethodAdaptationPlans = BuildMotorYMethodDecisions(detail.ItemDetails)
                .Select(snapshot => MotorYMethodAdaptationPlanContractMapper.Map(
                    snapshot,
                    MapBuildProfile,
                    BuildLegacyCodeDistributions(detail.ItemDetails, snapshot.CanonicalCode)))
                .Select(MapAdaptationPlanSnapshot)
                .ToArray(),
            ReportSummaries = detail.ReportSummaries
        };
    }

    private static IReadOnlyList<MotorYMethodDecisionSnapshot> BuildMotorYMethodDecisions(IReadOnlyList<TestRecordItemDetail> itemDetails)
    {
        return itemDetails
            .Select(item => item.BuildProfile ?? (item.LegacyAlgorithmRoute is null
                ? null
                : MotorYTrialItemBuildProfile.FromRoute(item.LegacyAlgorithmRoute)))
            .Where(profile => profile is not null && !string.IsNullOrWhiteSpace(profile.CanonicalCode))
            .Select(profile => profile!)
            .GroupBy(profile => profile.CanonicalCode, StringComparer.Ordinal)
            .Select(group =>
            {
                var totalCount = group.Count();
                var methodGroups = group
                    .GroupBy(profile => profile.MethodValue)
                    .Select(methodGroup => new
                    {
                        MethodValue = methodGroup.Key,
                        Count = methodGroup.Count(),
                        Profile = methodGroup.First()
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Profile.IsBaselineMethod ? 1 : 0)
                    .ThenBy(x => x.MethodValue)
                    .ToArray();
                var dominant = methodGroups[0];
                var baseline = group.FirstOrDefault(profile => profile.IsBaselineMethod)
                    ?? dominant.Profile;
                var baselineCount = group.Count(profile => profile.MethodValue == baseline.MethodValue);
                var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(baseline.CanonicalCode, baseline.MethodValue);
                var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(dominant.Profile.CanonicalCode, dominant.MethodValue);
                var distributions = methodGroups
                    .Select(row => new MotorYMethodDistributionSnapshot
                    {
                        MethodValue = row.MethodValue,
                        Count = row.Count,
                        Share = totalCount <= 0
                            ? 0d
                            : Math.Round((double)row.Count / totalCount, 4, MidpointRounding.AwayFromZero),
                        Route = MotorYLegacyAlgorithmRouteResolver.Resolve(row.Profile.CanonicalCode, row.MethodValue)
                    })
                    .ToArray();

                return MotorYMethodDecisionFactory.Create(
                    group.Key,
                    totalCount,
                    baselineRoute,
                    baselineCount,
                    dominantRoute,
                    dominant.Count,
                    distributions,
                    MotorYDominantOverrideThreshold);
            })
            .OrderBy(x => x.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<MotorYLegacyCodeDistributionSnapshot> BuildLegacyCodeDistributions(
        IReadOnlyList<TestRecordItemDetail> items,
        string canonicalCode)
    {
        var relevantItems = items
            .Where(item => string.Equals(item.ItemCode, canonicalCode, StringComparison.Ordinal))
            .ToArray();
        if (relevantItems.Length == 0)
        {
            return Array.Empty<MotorYLegacyCodeDistributionSnapshot>();
        }

        var candidateLegacyCodes = relevantItems
            .SelectMany(item => EnumerateLegacyCodeCandidates(item, canonicalCode))
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToArray();

        if (candidateLegacyCodes.Length == 0)
        {
            return Array.Empty<MotorYLegacyCodeDistributionSnapshot>();
        }

        return candidateLegacyCodes
            .GroupBy(code => code, StringComparer.Ordinal)
            .Select(group => new MotorYLegacyCodeDistributionSnapshot
            {
                CanonicalCode = canonicalCode,
                LegacyCode = group.Key,
                Count = group.Count(),
                Share = Math.Round((double)group.Count() / candidateLegacyCodes.Length, 4, MidpointRounding.AwayFromZero)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> EnumerateLegacyCodeCandidates(TestRecordItemDetail item, string canonicalCode)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.DisplayName)
            && !string.Equals(item.DisplayName, canonicalCode, StringComparison.Ordinal)
            && string.Equals(MotorYLegacyItemCodeNormalizer.Normalize(item.DisplayName), canonicalCode, StringComparison.Ordinal))
        {
            candidates.Add(item.DisplayName);
        }

        if (item.BuildProfile is not null)
        {
            var aliases = ResolveLegacyCodeAliases(canonicalCode, item.BuildProfile.MethodValue, item.BuildProfile.IsBaselineMethod);
            foreach (var alias in aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    candidates.Add(alias);
                }
            }
        }

        if (candidates.Count == 0)
        {
            candidates.AddRange(MotorYLegacyItemCodeNormalizer.GetLegacyAliases(canonicalCode));
        }

        return candidates
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> ResolveLegacyCodeAliases(string canonicalCode, int methodValue, bool isBaselineMethod)
    {
        if (isBaselineMethod)
        {
            return MotorYLegacyItemCodeNormalizer.GetLegacyAliases(canonicalCode);
        }

        return (canonicalCode, methodValue) switch
        {
            var key when key == (MotorYTestMethodCodes.DcResistance, 35) => new[] { "直流电阻测定（出厂）" },
            var key when key == (MotorYTestMethodCodes.DcResistance, 53) => new[] { "陪试直流电阻测定" },
            var key when key == (MotorYTestMethodCodes.DcResistance, 54) => new[] { "陪试直流电阻测定（出厂）" },
            var key when key == (MotorYTestMethodCodes.NoLoad, 59) => new[] { "空载试验（出厂）" },
            var key when key == (MotorYTestMethodCodes.HeatRun, 47) => new[] { "热试验（陪试）" },
            var key when key == (MotorYTestMethodCodes.HeatRun, 48) => new[] { "热试验2" },
            var key when key == (MotorYTestMethodCodes.LoadA, 60) => new[] { "A法负载试验（出厂）" },
            var key when key == (MotorYTestMethodCodes.LoadA, 61) => new[] { "A法负载试验（扩展）" },
            var key when key == (MotorYTestMethodCodes.LoadB, 51) => new[] { "B法负载试验（出厂）" },
            var key when key == (MotorYTestMethodCodes.LoadB, 52) => new[] { "B法负载试验（扩展）" },
            var key when key == (MotorYTestMethodCodes.LockedRotor, 46) => new[] { "堵转试验（出厂）" },
            var key when key == (MotorYTestMethodCodes.LockedRotor, 47) => new[] { "堵转试验（历史别名）" },
            _ => MotorYLegacyItemCodeNormalizer.GetLegacyAliases(canonicalCode)
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

    private static MotorYMethodAdaptationPlanSnapshot MapAdaptationPlanSnapshot(MotorYMethodAdaptationPlanContract contract)
    {
        return new MotorYMethodAdaptationPlanSnapshot
        {
            CanonicalCode = contract.CanonicalCode,
            TotalCount = contract.TotalCount,
            BaselineRoute = contract.BaselineProfile is null ? null : MotorYLegacyAlgorithmRouteResolver.Resolve(contract.BaselineProfile.CanonicalCode, contract.BaselineProfile.MethodValue),
            BaselineCount = contract.BaselineCount,
            BaselineShare = contract.BaselineShare,
            DominantRoute = contract.DominantProfile is null ? null : MotorYLegacyAlgorithmRouteResolver.Resolve(contract.DominantProfile.CanonicalCode, contract.DominantProfile.MethodValue),
            DominantCount = contract.DominantCount,
            DominantShare = contract.DominantShare,
            SelectedRoute = contract.SelectedProfile is null ? null : MotorYLegacyAlgorithmRouteResolver.Resolve(contract.SelectedProfile.CanonicalCode, contract.SelectedProfile.MethodValue),
            SelectedCount = contract.SelectedCount,
            SelectedShare = contract.SelectedShare,
            SelectionStrategy = contract.SelectionStrategy,
            ShouldUseDominantRoute = contract.ShouldUseDominantRoute,
            DominantOverrideThreshold = contract.DominantOverrideThreshold,
            DominantLeadCount = contract.DominantLeadCount,
            DominantLeadPercentagePoints = contract.DominantLeadPercentagePoints,
            SelectedLeadCountVsBaseline = contract.SelectedLeadCountVsBaseline,
            SelectedLeadPercentagePointsVsBaseline = contract.SelectedLeadPercentagePointsVsBaseline,
            SelectionReason = contract.SelectionReason,
            AlgorithmEntry = contract.AlgorithmEntry,
            SettingsMethodName = contract.SettingsMethodName,
            LegacyMethodName = contract.LegacyMethodName,
            RecommendedLegacyCode = contract.RecommendedLegacyCode,
            DominantLegacyCode = contract.DominantLegacyCode,
            RecommendedLegacyCodeCount = contract.RecommendedLegacyCodeCount,
            RecommendedLegacyCodeShare = contract.RecommendedLegacyCodeShare,
            LegacyCodeSelectionSummary = contract.LegacyCodeSelectionSummary,
            LegacyCodeDistributions = contract.LegacyCodeDistributions.Select(x => new MotorYLegacyCodeDistributionSnapshot
            {
                CanonicalCode = x.CanonicalCode,
                LegacyCode = x.LegacyCode,
                Count = x.Count,
                Share = x.Share
            }).ToArray(),
            RequiresRatedParams = contract.RequiresRatedParams,
            UpstreamCanonicalCodes = contract.UpstreamCanonicalCodes,
            UpstreamLegacyAliases = contract.UpstreamLegacyAliases,
            UpstreamLegacyCodeDistributions = contract.UpstreamLegacyCodeDistributions.Select(x => new MotorYLegacyUpstreamCodeDistributionSnapshot
            {
                CanonicalCode = x.CanonicalCode,
                LegacyCode = x.LegacyCode,
                Count = x.Count,
                Share = x.Share
            }).ToArray(),
            ObservedUpstreamCanonicalCodeCount = contract.ObservedUpstreamCanonicalCodeCount,
            ObservedUpstreamCanonicalCodes = contract.ObservedUpstreamCanonicalCodes,
            ObservedUpstreamLegacyCodes = contract.ObservedUpstreamLegacyCodes,
            MissingUpstreamCanonicalCodes = contract.MissingUpstreamCanonicalCodes,
            UpstreamDependenciesSatisfied = contract.UpstreamDependenciesSatisfied,
            UpstreamDependencySummary = contract.UpstreamDependencySummary,
            RequiredPayloadFields = contract.RequiredPayloadFields,
            RequiredRatedParamFields = contract.RequiredRatedParamFields,
            RequiredResultFields = contract.RequiredResultFields,
            RequiredIntermediateResultFields = contract.RequiredIntermediateResultFields,
            CoveredRequiredIntermediateResultFieldCount = contract.CoveredRequiredIntermediateResultFieldCount,
            MissingRequiredIntermediateResultFieldCount = contract.MissingRequiredIntermediateResultFieldCount,
            MissingRequiredIntermediateResultFields = contract.MissingRequiredIntermediateResultFields,
            CoveredRequiredIntermediateResultFields = contract.CoveredRequiredIntermediateResultFields,
            RequiredIntermediateResultFieldCoverageRatio = contract.RequiredIntermediateResultFieldCoverageRatio,
            RequiredIntermediateResultFieldCoveragePercentagePoints = contract.RequiredIntermediateResultFieldCoveragePercentagePoints,
            RequiredIntermediateResultFieldCoverageSummary = contract.RequiredIntermediateResultFieldCoverageSummary,
            CoveredRequiredResultFieldCount = contract.CoveredRequiredResultFieldCount,
            MissingRequiredResultFieldCount = contract.MissingRequiredResultFieldCount,
            MissingRequiredResultFields = contract.MissingRequiredResultFields,
            CoveredRequiredResultFields = contract.CoveredRequiredResultFields,
            RequiredResultFieldCoverageRatio = contract.RequiredResultFieldCoverageRatio,
            RequiredResultFieldCoveragePercentagePoints = contract.RequiredResultFieldCoveragePercentagePoints,
            RequiredResultFieldCoverageSummary = contract.RequiredResultFieldCoverageSummary,
            CoveredRequiredPayloadFieldCount = contract.CoveredRequiredPayloadFieldCount,
            MissingRequiredPayloadFieldCount = contract.MissingRequiredPayloadFieldCount,
            MissingRequiredPayloadFields = contract.MissingRequiredPayloadFields,
            CoveredRequiredPayloadFields = contract.CoveredRequiredPayloadFields,
            RequiredPayloadFieldCoverageRatio = contract.RequiredPayloadFieldCoverageRatio,
            RequiredPayloadFieldCoveragePercentagePoints = contract.RequiredPayloadFieldCoveragePercentagePoints,
            SamplePayloadAvailable = contract.SamplePayloadAvailable,
            RequiredPayloadFieldCoverageSummary = contract.RequiredPayloadFieldCoverageSummary,
            RequiredRawDataSignals = contract.RequiredRawDataSignals,
            ObservedRawDataSignals = contract.ObservedRawDataSignals,
            MissingRawDataSignals = contract.MissingRawDataSignals,
            RawDataSignalCoveredCount = contract.RawDataSignalCoveredCount,
            RawDataSignalMissingCount = contract.RawDataSignalMissingCount,
            RawDataSampleCount = contract.RawDataSampleCount,
            RawDataListAvailable = contract.RawDataListAvailable,
            RawDataSignalCoverageRatio = contract.RawDataSignalCoverageRatio,
            RawDataSignalCoveragePercentagePoints = contract.RawDataSignalCoveragePercentagePoints,
            RawDataSignalCoverageSummary = contract.RawDataSignalCoverageSummary,
            CoveredRequiredRatedParamFieldCount = contract.CoveredRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFieldCount = contract.MissingRequiredRatedParamFieldCount,
            MissingRequiredRatedParamFields = contract.MissingRequiredRatedParamFields,
            CoveredRequiredRatedParamFields = contract.CoveredRequiredRatedParamFields,
            RequiredRatedParamFieldCoverageRatio = contract.RequiredRatedParamFieldCoverageRatio,
            RequiredRatedParamFieldCoveragePercentagePoints = contract.RequiredRatedParamFieldCoveragePercentagePoints,
            RatedParamsAvailable = contract.RatedParamsAvailable,
            RequiredRatedParamFieldCoverageSummary = contract.RequiredRatedParamFieldCoverageSummary,
            LegacyAlgorithmInputsReady = contract.LegacyAlgorithmInputsReady,
            ObservedAlgorithmInputFields = contract.ObservedAlgorithmInputFields,
            MissingAlgorithmInputFields = contract.MissingAlgorithmInputFields,
            ObservedAlgorithmInputFieldCount = contract.ObservedAlgorithmInputFieldCount,
            MissingAlgorithmInputFieldCount = contract.MissingAlgorithmInputFieldCount,
            AlgorithmInputFieldCoverageRatio = contract.AlgorithmInputFieldCoverageRatio,
            AlgorithmInputFieldCoveragePercentagePoints = contract.AlgorithmInputFieldCoveragePercentagePoints,
            AlgorithmInputFieldCoverageSummary = contract.AlgorithmInputFieldCoverageSummary,
            RawDataSignalsReady = contract.RawDataSignalsReady,
            MinimumRawSampleCount = contract.MinimumRawSampleCount,
            RawSampleCountReady = contract.RawSampleCountReady,
            RawSampleCountReadinessSummary = contract.RawSampleCountReadinessSummary,
            RawSampleCountGap = contract.RawSampleCountGap,
            RawSampleCountDecisionSummary = contract.RawSampleCountDecisionSummary,
            RequiredStructuredPayloadSignals = contract.RequiredStructuredPayloadSignals,
            MinimumStructuredPayloadSampleCount = contract.MinimumStructuredPayloadSampleCount,
            StructuredPayloadSampleCountReady = contract.StructuredPayloadSampleCountReady,
            StructuredPayloadSampleCountReadinessSummary = contract.StructuredPayloadSampleCountReadinessSummary,
            StructuredPayloadSampleCountGap = contract.StructuredPayloadSampleCountGap,
            StructuredPayloadSampleCountDecisionSummary = contract.StructuredPayloadSampleCountDecisionSummary,
            ObservedStructuredPayloadSignals = contract.ObservedStructuredPayloadSignals,
            MissingStructuredPayloadSignals = contract.MissingStructuredPayloadSignals,
            StructuredPayloadSignalCoveredCount = contract.StructuredPayloadSignalCoveredCount,
            StructuredPayloadSignalMissingCount = contract.StructuredPayloadSignalMissingCount,
            StructuredPayloadSampleCount = contract.StructuredPayloadSampleCount,
            StructuredPayloadAvailable = contract.StructuredPayloadAvailable,
            StructuredPayloadSignalCoverageRatio = contract.StructuredPayloadSignalCoverageRatio,
            StructuredPayloadSignalCoveragePercentagePoints = contract.StructuredPayloadSignalCoveragePercentagePoints,
            StructuredPayloadSignalCoverageSummary = contract.StructuredPayloadSignalCoverageSummary,
            RequiredStructuredResultSignals = contract.RequiredStructuredResultSignals,
            MinimumStructuredResultSampleCount = contract.MinimumStructuredResultSampleCount,
            StructuredResultSampleCountReady = contract.StructuredResultSampleCountReady,
            StructuredResultSampleCountReadinessSummary = contract.StructuredResultSampleCountReadinessSummary,
            StructuredResultSampleCountGap = contract.StructuredResultSampleCountGap,
            StructuredResultSampleCountDecisionSummary = contract.StructuredResultSampleCountDecisionSummary,
            ObservedStructuredResultSignals = contract.ObservedStructuredResultSignals,
            MissingStructuredResultSignals = contract.MissingStructuredResultSignals,
            StructuredResultSignalCoveredCount = contract.StructuredResultSignalCoveredCount,
            StructuredResultSignalMissingCount = contract.StructuredResultSignalMissingCount,
            StructuredResultSampleCount = contract.StructuredResultSampleCount,
            StructuredResultAvailable = contract.StructuredResultAvailable,
            StructuredResultSignalCoverageRatio = contract.StructuredResultSignalCoverageRatio,
            StructuredResultSignalCoveragePercentagePoints = contract.StructuredResultSignalCoveragePercentagePoints,
            StructuredResultSignalCoverageSummary = contract.StructuredResultSignalCoverageSummary,
            LegacyAlgorithmInputReadinessSummary = contract.LegacyAlgorithmInputReadinessSummary,
            DependencyNotes = contract.DependencyNotes,
            FormulaSignals = contract.FormulaSignals,
            CoveredFormulaSignalCount = contract.CoveredFormulaSignalCount,
            MissingFormulaSignalCount = contract.MissingFormulaSignalCount,
            CoveredFormulaSignals = contract.CoveredFormulaSignals,
            MissingFormulaSignals = contract.MissingFormulaSignals,
            FormulaSignalCoverageRatio = contract.FormulaSignalCoverageRatio,
            FormulaSignalCoveragePercentagePoints = contract.FormulaSignalCoveragePercentagePoints,
            FormulaSignalsBackedByObservedPayload = contract.FormulaSignalsBackedByObservedPayload,
            FormulaSignalsObservedPayloadFields = contract.FormulaSignalsObservedPayloadFields,
            FormulaSignalObservedPayloadGaps = contract.FormulaSignalObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapSnapshot
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            FormulaSignalsObservedPayloadSummary = contract.FormulaSignalsObservedPayloadSummary,
            LegacyAlgorithmRules = contract.LegacyAlgorithmRules,
            CoveredLegacyAlgorithmRuleCount = contract.CoveredLegacyAlgorithmRuleCount,
            MissingLegacyAlgorithmRuleCount = contract.MissingLegacyAlgorithmRuleCount,
            CoveredLegacyAlgorithmRules = contract.CoveredLegacyAlgorithmRules,
            MissingLegacyAlgorithmRules = contract.MissingLegacyAlgorithmRules,
            LegacyAlgorithmRuleCoverageRatio = contract.LegacyAlgorithmRuleCoverageRatio,
            LegacyAlgorithmRuleCoveragePercentagePoints = contract.LegacyAlgorithmRuleCoveragePercentagePoints,
            LegacyAlgorithmRulesBackedByObservedPayload = contract.LegacyAlgorithmRulesBackedByObservedPayload,
            LegacyAlgorithmRulesObservedPayloadFields = contract.LegacyAlgorithmRulesObservedPayloadFields,
            LegacyAlgorithmRulesObservedPayloadGaps = contract.LegacyAlgorithmRulesObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapSnapshot
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            LegacyAlgorithmRulesObservedPayloadSummary = contract.LegacyAlgorithmRulesObservedPayloadSummary,
            LegacyDecisionAnchors = contract.LegacyDecisionAnchors,
            CoveredLegacyDecisionAnchorCount = contract.CoveredLegacyDecisionAnchorCount,
            MissingLegacyDecisionAnchorCount = contract.MissingLegacyDecisionAnchorCount,
            CoveredLegacyDecisionAnchors = contract.CoveredLegacyDecisionAnchors,
            MissingLegacyDecisionAnchors = contract.MissingLegacyDecisionAnchors,
            LegacyDecisionAnchorCoverageRatio = contract.LegacyDecisionAnchorCoverageRatio,
            LegacyDecisionAnchorCoveragePercentagePoints = contract.LegacyDecisionAnchorCoveragePercentagePoints,
            LegacyDecisionAnchorsBackedByObservedPayload = contract.LegacyDecisionAnchorsBackedByObservedPayload,
            LegacyDecisionAnchorReady = contract.LegacyDecisionAnchorReady,
            LegacyDecisionAnchorsObservedPayloadFields = contract.LegacyDecisionAnchorsObservedPayloadFields,
            LegacyDecisionAnchorsObservedPayloadGaps = contract.LegacyDecisionAnchorsObservedPayloadGaps.Select(x => new MotorYObservedAlgorithmEvidenceGapSnapshot
            {
                SignalOrRule = x.SignalOrRule,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            LegacyDecisionAnchorObservationRules = contract.LegacyDecisionAnchorObservationRules.Select(x => new MotorYDecisionAnchorObservationRuleSnapshot
            {
                AnchorKey = x.AnchorKey,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoveredByObservedPayload = x.CoveredByObservedPayload,
                Summary = x.Summary
            }).ToArray(),
            LegacyDecisionAnchorResolutions = contract.LegacyDecisionAnchorResolutions.Select(x => new MotorYDecisionAnchorResolutionSnapshot
            {
                AnchorKey = x.AnchorKey,
                ResolvedByObservedPayload = x.ResolvedByObservedPayload,
                PartiallyResolvedByObservedPayload = x.PartiallyResolvedByObservedPayload,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoverageRatio = x.CoverageRatio,
                CoveragePercentagePoints = x.CoveragePercentagePoints,
                ResolutionStage = x.ResolutionStage,
                SuggestedNextStepCategory = x.SuggestedNextStepCategory,
                SuggestedNextStepFocus = x.SuggestedNextStepFocus,
                SuggestedNextStepFields = x.SuggestedNextStepFields,
                SuggestedNextSteps = x.SuggestedNextSteps,
                SuggestedNextStepSummary = x.SuggestedNextStepSummary,
                SuggestedNextStepPriority = x.SuggestedNextStepPriority,
                SuggestedNextStepPrioritySummary = x.SuggestedNextStepPrioritySummary,
                SuggestedNextStepCoverageSummary = x.SuggestedNextStepCoverageSummary,
                SuggestedPrimaryNextField = x.SuggestedPrimaryNextField,
                SuggestedPrimaryNextFieldSummary = x.SuggestedPrimaryNextFieldSummary,
                Summary = x.Summary
            }).ToArray(),
            CoveredLegacyDecisionAnchorObservationRuleCount = contract.CoveredLegacyDecisionAnchorObservationRuleCount,
            MissingLegacyDecisionAnchorObservationRuleCount = contract.MissingLegacyDecisionAnchorObservationRuleCount,
            ResolvedLegacyDecisionAnchorCount = contract.ResolvedLegacyDecisionAnchorCount,
            PartialLegacyDecisionAnchorCount = contract.PartialLegacyDecisionAnchorCount,
            MissingLegacyDecisionAnchorResolutionCount = contract.MissingLegacyDecisionAnchorResolutionCount,
            EffectiveLegacyDecisionAnchorCoverageCount = contract.EffectiveLegacyDecisionAnchorCoverageCount,
            EffectiveLegacyDecisionAnchorGapCount = contract.EffectiveLegacyDecisionAnchorGapCount,
            LegacyDecisionAnchorObservationRuleCoverageRatio = contract.LegacyDecisionAnchorObservationRuleCoverageRatio,
            LegacyDecisionAnchorObservationRuleCoveragePercentagePoints = contract.LegacyDecisionAnchorObservationRuleCoveragePercentagePoints,
            LegacyDecisionAnchorResolutionCoverageRatio = contract.LegacyDecisionAnchorResolutionCoverageRatio,
            LegacyDecisionAnchorResolutionCoveragePercentagePoints = contract.LegacyDecisionAnchorResolutionCoveragePercentagePoints,
            EffectiveLegacyDecisionAnchorCoverageRatio = contract.EffectiveLegacyDecisionAnchorCoverageRatio,
            EffectiveLegacyDecisionAnchorCoveragePercentagePoints = contract.EffectiveLegacyDecisionAnchorCoveragePercentagePoints,
            LegacyDecisionAnchorObservationRuleSummary = contract.LegacyDecisionAnchorObservationRuleSummary,
            LegacyDecisionAnchorResolutionSummary = contract.LegacyDecisionAnchorResolutionSummary,
            LegacyDecisionAnchorNextActionSummary = contract.LegacyDecisionAnchorNextActionSummary,
            LegacyDecisionAnchorGapPreviewSummary = contract.LegacyDecisionAnchorGapPreviewSummary,
            DecisionAnchorPriorityDistributions = contract.DecisionAnchorPriorityDistributions.Select(x => new MotorYDecisionAnchorPriorityDistributionSnapshot
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
            DecisionAnchorPrioritySummary = contract.DecisionAnchorPrioritySummary,
            SuggestedDecisionAnchorNextSteps = contract.SuggestedDecisionAnchorNextSteps,
            SuggestedDecisionAnchorNextStepSummary = contract.SuggestedDecisionAnchorNextStepSummary,
            LegacyDecisionAnchorsObservedPayloadSummary = contract.LegacyDecisionAnchorsObservedPayloadSummary,
            FormulaSignalSummary = contract.FormulaSignalSummary,
            LegacyAlgorithmRuleSummary = contract.LegacyAlgorithmRuleSummary,
            LegacyDecisionAnchorSummary = contract.LegacyDecisionAnchorSummary,
            SelectedMethodSummary = contract.SelectedMethodSummary,
            BaselineDominantComparisonSummary = contract.BaselineDominantComparisonSummary,
            DependencyBuckets = contract.DependencyBuckets.Select(x => new MotorYDependencyBucketSummarySnapshot
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
            Distributions = contract.Distributions.Select(x => new MotorYMethodDistributionSnapshot
            {
                MethodValue = x.MethodValue,
                Count = x.Count,
                Share = x.Share,
                Route = x.Profile is null ? null : MotorYLegacyAlgorithmRouteResolver.Resolve(x.Profile.CanonicalCode, x.Profile.MethodValue)
            }).ToArray()
        };
    }

    private static string BuildProductDisplayName(string? model, string? code, string productKind)
    {
        if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(code))
        {
            return $"{model} ({code})";
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            return model;
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        return productKind;
    }
}
