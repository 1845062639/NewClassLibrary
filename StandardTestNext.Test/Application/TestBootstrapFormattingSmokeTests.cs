using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application;

public static class TestBootstrapFormattingSmokeTests
{
    public static void Run()
    {
        ShouldExposeDecisionAnchorSuggestedNextStepInCliPreview();
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
                    SuggestedNextStepSummary = "先补决策锚点 rconverse-branch: RConverseType"
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

        if (!formatted.Contains("anchor-resolutions=rconverse-branch:missing:0pp:obs=none:miss=RConverseType:next=先补决策锚点 rconverse-branch: RConverseType", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap formatting smoke test failed. actual='{formatted}'");
        }
    }
}
