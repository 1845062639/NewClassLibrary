using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application;

public static class TestBootstrapFormattingSmokeTests
{
    public static void Run()
    {
        ShouldExposeDecisionAnchorSuggestedNextStepInCliPreview();
        ShouldExposeDecisionAnchorPriorityAndCoverageInCliPreview();
    }

    private static void ShouldExposeDecisionAnchorSuggestedNextStepInCliPreview()
    {
        var plan = new MotorYMethodAdaptationPlanSnapshot
        {
            CanonicalCode = MotorYTestMethodCodes.NoLoad,
            SelectionStrategy = "baseline",
            AlgorithmEntry = "Calc_NoLoad",
            SettingsMethodName = "空载试验",
            SelectionReason = "smoke",
            RawSampleCountReady = true,
            RawDataSampleCount = 3,
            MinimumRawSampleCount = 1,
            StructuredPayloadSampleCountReady = false,
            StructuredPayloadSampleCount = 0,
            MinimumStructuredPayloadSampleCount = 1,
            StructuredResultSampleCountReady = false,
            StructuredResultSampleCount = 0,
            MinimumStructuredResultSampleCount = 1,
            LegacyDecisionAnchorResolutions = new[]
            {
                new MotorYDecisionAnchorResolutionSnapshot
                {
                    AnchorKey = "rconverse-branch",
                    ResolutionStage = "missing",
                    CoveragePercentagePoints = 0,
                    MissingPayloadFields = new[] { "RConverseType" },
                    SuggestedNextStepSummary = "先补空载旧算法的 R0/θ0 换算分支标记：RConverseType"
                }
            },
            SuggestedNextStepSummary = "先补决策锚点观测依据: rconverse-branch",
            SuggestedDecisionAnchorNextStepSummary = "先补决策锚点 rconverse-branch: RConverseType",
            LegacyDecisionAnchorGapPreviewSummary = "decision anchor gaps: rconverse-branch[missing]:RConverseType",
            LegacyDecisionAnchorResolutionSummary = "decision anchor resolutions resolved 0/1 (0pp); partial=0; missing=1; unresolved: rconverse-branch:missing",
            LegacyAlgorithmInputReadinessSummary = "legacy algorithm inputs incomplete",
            SelectedMethodSummary = "推荐方法沿用 baseline",
            BaselineDominantComparisonSummary = "baseline 与 dominant 一致"
        };

        var formatter = typeof(TestBootstrap).GetMethod("FormatMethodAdaptationPlanSnapshot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap formatter not found.");
        var formatted = formatter.Invoke(null, new object[] { plan }) as string
            ?? throw new InvalidOperationException("TestBootstrap formatter returned null.");

        if (!formatted.Contains("anchor-resolutions=rconverse-branch:missing:0pp:obs=none:miss=RConverseType:priority=none:coverage=none:next=先补空载旧算法的 R0/θ0 换算分支标记：RConverseType", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap formatting smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldExposeDecisionAnchorPriorityAndCoverageInCliPreview()
    {
        var plan = new MotorYMethodAdaptationPlanSnapshot
        {
            CanonicalCode = MotorYTestMethodCodes.LoadB,
            SelectionStrategy = "dominant",
            AlgorithmEntry = "Calc_Load_B",
            SettingsMethodName = "B法负载试验",
            SelectionReason = "smoke",
            RawSampleCountReady = true,
            RawDataSampleCount = 6,
            MinimumRawSampleCount = 3,
            StructuredPayloadSampleCountReady = true,
            StructuredPayloadSampleCount = 4,
            MinimumStructuredPayloadSampleCount = 3,
            StructuredResultSampleCountReady = false,
            StructuredResultSampleCount = 0,
            MinimumStructuredResultSampleCount = 1,
            DecisionAnchorPrioritySummary = "decision anchor priorities: blocking=1/1 (100pp) anchors [ps-iteration], focus B法 Ps 非负迭代收敛字段",
            DecisionAnchorPriorityDistributions = new[]
            {
                new MotorYDecisionAnchorPriorityDistributionSnapshot
                {
                    Priority = "blocking",
                    Count = 1,
                    Share = 1d,
                    AnchorKeys = new[] { "ps-iteration" },
                    SuggestedNextStepFocuses = new[] { "B法 Ps 非负迭代收敛字段" },
                    SuggestedNextStepFields = new[] { "Ps", "cuC" },
                    SuggestedNextSteps = new[] { "继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC" },
                    SuggestedNextStepSummary = "priority blocking next steps: 继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC",
                    DominantAnchorKey = "ps-iteration",
                    DominantSuggestedNextStepFocus = "B法 Ps 非负迭代收敛字段",
                    DominantSuggestedNextStepFields = new[] { "Ps", "cuC" },
                    DominantSuggestedNextStepSummary = "继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC"
                }
            },
            LegacyDecisionAnchorResolutions = new[]
            {
                new MotorYDecisionAnchorResolutionSnapshot
                {
                    AnchorKey = "ps-iteration",
                    ResolutionStage = "missing",
                    CoveragePercentagePoints = 33,
                    RequiredPayloadFields = new[] { "ResultDataList", "Ps", "cuC" },
                    ObservedPayloadFields = new[] { "ResultDataList" },
                    MissingPayloadFields = new[] { "Ps", "cuC" },
                    SuggestedNextStepSummary = "继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC",
                    SuggestedNextStepPriority = "blocking",
                    SuggestedNextStepPrioritySummary = "B法 Ps 非负迭代收敛字段仍阻塞旧算法决策分支，建议优先补齐",
                    SuggestedNextStepCoverageSummary = "decision anchor coverage 1/3 (33pp); missing: Ps, cuC"
                }
            },
            SuggestedDecisionAnchorNextSteps = new[] { "继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC" },
            SuggestedNextStepSummary = "先补旧算法分支证据",
            SuggestedDecisionAnchorNextStepSummary = "继续补齐 B 法 Ps 迭代字段",
            LegacyDecisionAnchorGapPreviewSummary = "decision anchor gaps: ps-iteration[partial]:Ps, cuC",
            LegacyDecisionAnchorResolutionSummary = "decision anchor resolutions resolved 0/1 (0pp); partial=1; missing=0; unresolved: ps-iteration:partial",
            LegacyAlgorithmInputReadinessSummary = "legacy algorithm inputs incomplete",
            SelectedMethodSummary = "推荐方法切到 dominant",
            BaselineDominantComparisonSummary = "dominant 明显领先 baseline"
        };

        var formatter = typeof(TestBootstrap).GetMethod("FormatMethodAdaptationPlanSnapshot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap formatter not found.");
        var formatted = formatter.Invoke(null, new object[] { plan }) as string
            ?? throw new InvalidOperationException("TestBootstrap formatter returned null.");

        if (!formatted.Contains("anchor-priority=blocking:1:100.0 %:ps-iteration:B法 Ps 非负迭代收敛字段:fields=Ps|cuC:top=ps-iteration:B法 Ps 非负迭代收敛字段:top-fields=Ps|cuC:next=priority blocking next steps: 继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC", StringComparison.Ordinal)
            || !formatted.Contains("anchor-resolutions=ps-iteration:missing:33pp:obs=ResultDataList:miss=Ps+cuC:priority=blocking:coverage=decision anchor coverage 1/3 (33pp); missing: Ps, cuC:next=继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC", StringComparison.Ordinal)
            || !formatted.Contains("priority-summary=decision anchor priorities: blocking=1/1 (100pp) anchors [ps-iteration], focus B法 Ps 非负迭代收敛字段", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap priority formatting smoke test failed. actual='{formatted}'");
        }
    }
}
