using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordViewMapper
{
    private const double MotorYDominantOverrideThreshold = 0.7d;

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
                    .ThenBy(x => x.MethodValue)
                    .ToArray();
                var dominant = methodGroups[0];
                var baseline = group.FirstOrDefault(profile => profile.IsBaselineMethod)
                    ?? dominant.Profile;
                var baselineCount = group.Count(profile => profile.MethodValue == baseline.MethodValue);
                var dominantShare = totalCount <= 0
                    ? 0d
                    : Math.Round((double)dominant.Count / totalCount, 4, MidpointRounding.AwayFromZero);
                var baselineShare = totalCount <= 0
                    ? 0d
                    : Math.Round((double)baselineCount / totalCount, 4, MidpointRounding.AwayFromZero);
                var dominantLeadCount = Math.Max(0, dominant.Count - baselineCount);
                var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((dominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
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
                var shouldPrioritizeDominant = dominant.MethodValue != baseline.MethodValue
                    && dominantShare >= MotorYDominantOverrideThreshold;
                var recommendedRoute = shouldPrioritizeDominant
                    ? MotorYLegacyAlgorithmRouteResolver.Resolve(dominant.Profile.CanonicalCode, dominant.MethodValue)
                    : MotorYLegacyAlgorithmRouteResolver.Resolve(baseline.CanonicalCode, baseline.MethodValue);
                var recommendedStrategy = shouldPrioritizeDominant
                    ? "dominant-threshold-over-baseline"
                    : "baseline";

                var decision = new MotorYMethodDecisionSnapshot
                {
                    CanonicalCode = group.Key,
                    TotalCount = totalCount,
                    BaselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(baseline.CanonicalCode, baseline.MethodValue),
                    BaselineCount = baselineCount,
                    DominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(dominant.Profile.CanonicalCode, dominant.MethodValue),
                    DominantCount = dominant.Count,
                    RecommendedRoute = recommendedRoute,
                    RecommendedStrategy = recommendedStrategy,
                    ShouldPrioritizeDominantOverBaseline = shouldPrioritizeDominant,
                    DominantShare = dominantShare,
                    BaselineShare = baselineShare,
                    DominantOverrideThreshold = MotorYDominantOverrideThreshold,
                    DominantLeadCount = dominantLeadCount,
                    DominantLeadPercentagePoints = dominantLeadPercentagePoints,
                    RecommendationReason = shouldPrioritizeDominant
                        ? $"selected dominant method {dominant.MethodValue} over baseline {baseline.MethodValue} because dominant share {dominantShare:P2} reached threshold {MotorYDominantOverrideThreshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)"
                        : baseline.MethodValue == dominant.MethodValue
                            ? $"kept baseline method {baseline.MethodValue} because baseline already matches dominant distribution ({dominantShare:P2})"
                            : $"kept baseline method {baseline.MethodValue} because dominant method {dominant.MethodValue} share {dominantShare:P2} did not reach threshold {MotorYDominantOverrideThreshold:P0}",
                    Distributions = distributions
                };

                var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(decision);
                return new MotorYMethodDecisionSnapshot
                {
                    CanonicalCode = decision.CanonicalCode,
                    TotalCount = decision.TotalCount,
                    BaselineRoute = decision.BaselineRoute,
                    BaselineCount = decision.BaselineCount,
                    DominantRoute = decision.DominantRoute,
                    DominantCount = decision.DominantCount,
                    RecommendedRoute = decision.RecommendedRoute,
                    RecommendedStrategy = decision.RecommendedStrategy,
                    ShouldPrioritizeDominantOverBaseline = decision.ShouldPrioritizeDominantOverBaseline,
                    DominantShare = decision.DominantShare,
                    BaselineShare = decision.BaselineShare,
                    DominantOverrideThreshold = decision.DominantOverrideThreshold,
                    DominantLeadCount = decision.DominantLeadCount,
                    DominantLeadPercentagePoints = decision.DominantLeadPercentagePoints,
                    RecommendationReason = decision.RecommendationReason,
                    RecommendedMethodSummary = selection.SelectedMethodSummary,
                    BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
                    Distributions = decision.Distributions
                };
            })
            .OrderBy(x => x.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
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
