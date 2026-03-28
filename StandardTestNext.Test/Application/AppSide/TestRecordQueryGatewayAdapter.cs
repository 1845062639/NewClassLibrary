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
            DominantOverrideThreshold = snapshot.DominantOverrideThreshold,
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
