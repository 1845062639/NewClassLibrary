using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application;

public static class TestBootstrapFormattingSmokeTests
{
    public static void Run()
    {
        ShouldExposeDecisionAnchorSuggestedNextStepInCliPreview();
        ShouldExposeDecisionAnchorPriorityAndCoverageInCliPreview();
        ShouldFormatCrossPlanRequiredResultPrimaryFieldFocuses();
        ShouldExposeWeightedCrossPlanDecisionAnchorFieldsInCliPlanPreview();
        ShouldExposeWeightedCrossPlanRequiredResultFieldsInCliPlanPreview();
        ShouldFormatAlgorithmFamilyPrimaryFieldFocuses();
        ShouldExposeAlgorithmFamilyPrimaryFieldFocusesInCliPlanPreview();
        ShouldBuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses();
        ShouldBuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses();
        ShouldBuildVariantKindDecisionAnchorPrimaryFieldFocuses();
        ShouldBuildVariantKindRequiredResultPrimaryFieldFocuses();
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

    private static void ShouldFormatCrossPlanRequiredResultPrimaryFieldFocuses()
    {
        var formatter = typeof(TestBootstrap).GetMethod("FormatCrossPlanPrimaryFieldFocuses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap cross-plan formatter not found.");

        var formatted = formatter.Invoke(null, new object[]
        {
            new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "Pfw",
                    Count = 2,
                    Share = 1d,
                    WeightedCount = 7,
                    WeightedShare = 0.7d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad },
                    MethodKeys = new[] { "LoadB:5", "NoLoad:0" },
                    LegacyAlgorithmEntries = new[] { "Calc_Load_B", "Calc_NoLoad" },
                    SuggestedNextStepPriorities = new[] { "intermediate-result-fields", "result-fields" },
                    SuggestedNextStepFocuses = new[] { "NoLoad result field", "LoadB result field" },
                    Summary = "cross-plan primary field Pfw appears in 2/2 plans (100pp), weighted 7/10 selected samples (70pp); codes=MotorY.LoadB, MotorY.NoLoad; methods=none; method-keys=LoadB:5, NoLoad:0; profiles=none; legacy-methods=none; settings-methods=none; algo-entries=Calc_Load_B, Calc_NoLoad; families=none; variants=none; focuses=NoLoad result field, LoadB result field; priorities=intermediate-result-fields, result-fields"
                }
            }
        }) as string ?? throw new InvalidOperationException("TestBootstrap cross-plan formatter returned null.");

        if (!formatted.Contains("Pfw:2:100.0 %:weighted=7/10:70.0 %:methods=none:method-keys=LoadB:5/NoLoad:0:profiles=none:legacy-methods=none:settings-methods=none:algo-entries=Calc_Load_B|Calc_NoLoad:MotorY.LoadB/MotorY.NoLoad:LoadB:5/NoLoad:0:intermediate-result-fields/result-fields:summary=cross-plan primary field Pfw appears in 2/2 plans (100pp), weighted 7/10 selected samples (70pp); codes=MotorY.LoadB, MotorY.NoLoad; methods=none; method-keys=LoadB:5, NoLoad:0; profiles=none; legacy-methods=none; settings-methods=none; algo-entries=Calc_Load_B, Calc_NoLoad; families=none; variants=none; focuses=NoLoad result field, LoadB result field; priorities=intermediate-result-fields, result-fields", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap cross-plan required-result formatter smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldExposeWeightedCrossPlanDecisionAnchorFieldsInCliPlanPreview()
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
            StructuredPayloadSampleCountReady = true,
            StructuredPayloadSampleCount = 2,
            MinimumStructuredPayloadSampleCount = 1,
            StructuredResultSampleCountReady = true,
            StructuredResultSampleCount = 2,
            MinimumStructuredResultSampleCount = 1,
            CrossPlanDecisionAnchorPrimaryFieldSummary = "cross-plan decision-anchor primary fields top 1/3: GB=2 (100pp, weighted 70pp)",
            CrossPlanDecisionAnchorPrimaryFieldFocuses = new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "GB",
                    Count = 2,
                    Share = 1d,
                    WeightedCount = 7,
                    WeightedShare = 0.7d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad },
                    AlgorithmFamilies = new[] { "LoadB", "NoLoad" },
                    MethodKeys = new[] { "LoadB:5", "NoLoad:0" },
                    LegacyMethodNames = new[] { "B法负载试验", "空载试验" },
                    SettingsMethodNames = new[] { "B法负载试验", "空载试验" },
                    LegacyAlgorithmEntries = new[] { "Calc_Load_B", "Calc_NoLoad" },
                    AnchorKeys = new[] { "gb-branch", "gb-compare" },
                    SuggestedNextStepPriorities = new[] { "blocking", "decision-branch" },
                    SuggestedNextStepFocuses = new[] { "旧算法GB分支", "旧算法GB比较" },
                    Summary = "cross-plan primary field GB appears in 2/2 plans (100pp), weighted 7/10 selected samples (70pp); codes=MotorY.LoadB, MotorY.NoLoad; focuses=旧算法GB分支, 旧算法GB比较; priorities=blocking, decision-branch"
                }
            },
            LegacyDecisionAnchorResolutionSummary = "decision anchor resolutions resolved 0/0 (100pp)",
            LegacyAlgorithmInputReadinessSummary = "legacy algorithm inputs ready",
            SelectedMethodSummary = "推荐方法沿用 baseline",
            BaselineDominantComparisonSummary = "baseline 与 dominant 一致"
        };

        var formatter = typeof(TestBootstrap).GetMethod("FormatMethodAdaptationPlanSnapshot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap formatter not found.");
        var formatted = formatter.Invoke(null, new object[] { plan }) as string
            ?? throw new InvalidOperationException("TestBootstrap formatter returned null.");

        if (!formatted.Contains("anchor-cross-plan=GB:2:100.0 %:weighted=7:70.0 %:methods=:method-keys=LoadB:5/NoLoad:0:profiles=:legacy-methods=B法负载试验/空载试验:settings-methods=B法负载试验/空载试验:algo-entries=Calc_Load_B/Calc_NoLoad:MotorY.LoadB/MotorY.NoLoad:families=LoadB/NoLoad:blocking/decision-branch:summary=cross-plan decision-anchor primary fields top 1/3: GB=2 (100pp, weighted 70pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap weighted cross-plan anchor-primary formatting smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldExposeWeightedCrossPlanRequiredResultFieldsInCliPlanPreview()
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
            StructuredPayloadSampleCountReady = true,
            StructuredPayloadSampleCount = 2,
            MinimumStructuredPayloadSampleCount = 1,
            StructuredResultSampleCountReady = true,
            StructuredResultSampleCount = 2,
            MinimumStructuredResultSampleCount = 1,
            CrossPlanRequiredResultPrimaryFieldSummary = "cross-plan required-result primary fields top 1/3: Pfw=2 (100pp, weighted 70pp)",
            CrossPlanRequiredResultPrimaryFieldFocuses = new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "Pfw",
                    Count = 2,
                    Share = 1d,
                    WeightedCount = 7,
                    WeightedShare = 0.7d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad },
                    AlgorithmFamilies = new[] { "LoadB", "NoLoad" },
                    MethodKeys = new[] { "LoadB:5", "NoLoad:0" },
                    SuggestedNextStepPriorities = new[] { "intermediate-result-fields", "result-fields" },
                    SuggestedNextStepFocuses = new[] { "中间结果锚点", "结果字段" },
                    Summary = "cross-plan primary field Pfw appears in 2/2 plans (100pp), weighted 7/10 selected samples (70pp); codes=MotorY.LoadB, MotorY.NoLoad; families=LoadB, NoLoad; focuses=中间结果锚点, 结果字段; priorities=intermediate-result-fields, result-fields"
                }
            },
            LegacyDecisionAnchorResolutionSummary = "decision anchor resolutions resolved 0/0 (100pp)",
            LegacyAlgorithmInputReadinessSummary = "legacy algorithm inputs ready",
            SelectedMethodSummary = "推荐方法沿用 baseline",
            BaselineDominantComparisonSummary = "baseline 与 dominant 一致"
        };

        var formatter = typeof(TestBootstrap).GetMethod("FormatMethodAdaptationPlanSnapshot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap formatter not found.");
        var formatted = formatter.Invoke(null, new object[] { plan }) as string
            ?? throw new InvalidOperationException("TestBootstrap formatter returned null.");

        if (!formatted.Contains("result-cross-plan=Pfw:2:100.0 %:weighted=7:70.0 %:methods=LoadB:5/NoLoad:0:profiles=:MotorY.LoadB/MotorY.NoLoad:families=LoadB/NoLoad:intermediate-result-fields/result-fields:summary=cross-plan required-result primary fields top 1/3: Pfw=2 (100pp, weighted 70pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap weighted cross-plan result-primary formatting smoke test failed. actual='{formatted}'");
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
            DecisionAnchorTopPriority = "blocking",
            DecisionAnchorTopPrioritySummary = "top decision anchor priority=blocking; focus=B法 Ps 非负迭代收敛字段; anchor=ps-iteration; fields=Ps, cuC",
            DecisionAnchorTopPriorityDominantAnchorKey = "ps-iteration",
            DecisionAnchorTopPriorityFocus = "B法 Ps 非负迭代收敛字段",
            DecisionAnchorTopPriorityFields = new[] { "Ps", "cuC" },
            DecisionAnchorTopPriorityNextStepSummary = "继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC",
            DecisionAnchorTopPriorityPrimaryField = "Ps",
            DecisionAnchorTopPriorityPrimaryFieldSummary = "优先补字段 Ps，用于推进 B法 Ps 非负迭代收敛字段（ps-iteration）",
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
            DecisionAnchorPrimaryFieldDistributions = new[]
            {
                new MotorYDecisionAnchorPrimaryFieldDistributionSnapshot
                {
                    PrimaryField = "Ps",
                    Count = 1,
                    Share = 1d,
                    AnchorKeys = new[] { "ps-iteration" },
                    SuggestedNextStepFocuses = new[] { "B法 Ps 非负迭代收敛字段" },
                    SuggestedNextStepPriorities = new[] { "blocking" },
                    Summary = "primary field Ps referenced by 1/1 anchors (100pp); anchors=ps-iteration; priorities=blocking"
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
            || !formatted.Contains("anchor-top-priority=blocking:ps-iteration:B法 Ps 非负迭代收敛字段:fields=Ps|cuC:primary=Ps:primary-summary=优先补字段 Ps，用于推进 B法 Ps 非负迭代收敛字段（ps-iteration）:next=继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC:summary=top decision anchor priority=blocking; focus=B法 Ps 非负迭代收敛字段; anchor=ps-iteration; fields=Ps, cuC", StringComparison.Ordinal)
            || !formatted.Contains("anchor-resolutions=ps-iteration:missing:33pp:obs=ResultDataList:miss=Ps+cuC:priority=blocking:coverage=decision anchor coverage 1/3 (33pp); missing: Ps, cuC:next=继续补齐B法 Ps 非负迭代收敛字段：Ps, cuC", StringComparison.Ordinal)
            || !formatted.Contains("priority-summary=decision anchor priorities: blocking=1/1 (100pp) anchors [ps-iteration], focus B法 Ps 非负迭代收敛字段", StringComparison.Ordinal)
            || !formatted.Contains("anchor-primary=Ps:1:100.0 %:ps-iteration:blocking:summary=decision-anchor primary fields: Ps=1/1 (100pp) anchors [ps-iteration], focus B法 Ps 非负迭代收敛字段", StringComparison.Ordinal)
            || !formatted.Contains("primary field Ps referenced by 1/1 anchors (100pp); anchors=ps-iteration; priorities=blocking", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap priority formatting smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldFormatAlgorithmFamilyPrimaryFieldFocuses()
    {
        var formatter = typeof(TestBootstrap).GetMethod("FormatCrossPlanPrimaryFieldFocuses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap cross-plan formatter not found.");

        var formatted = formatter.Invoke(null, new object[]
        {
            new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "GB",
                    Count = 1,
                    Share = 1d,
                    WeightedCount = 100,
                    WeightedShare = 1d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB },
                    AlgorithmFamilies = new[] { "LoadB" },
                    MethodKeys = new[] { "LoadB:5" },
                    AnchorKeys = new[] { "gb-temperature-branch" },
                    SuggestedNextStepPriorities = new[] { "blocking" },
                    SuggestedNextStepFocuses = new[] { "热态分支" },
                    Summary = "family=LoadB; cross-plan primary field GB appears in 1/1 plans (100pp), weighted 100/100 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=热态分支; priorities=blocking"
                }
            }
        }) as string ?? throw new InvalidOperationException("TestBootstrap cross-plan formatter returned null.");

        if (!formatted.Contains("GB:1:100.0 %:weighted=100/100:100.0 %:methods=none:method-keys=LoadB:5:profiles=none:legacy-methods=none:settings-methods=none:MotorY.LoadB:LoadB:5:blocking:summary=family=LoadB; cross-plan primary field GB appears in 1/1 plans (100pp), weighted 100/100 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=热态分支; priorities=blocking", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap algorithm-family formatter smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldExposeAlgorithmFamilyPrimaryFieldFocusesInCliPlanPreview()
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
            StructuredResultSampleCountReady = true,
            StructuredResultSampleCount = 2,
            MinimumStructuredResultSampleCount = 1,
            AlgorithmFamilyDecisionAnchorPrimaryFieldSummary = "algorithm-family decision-anchor primary fields top 1/1: GB=1 (100pp, weighted 7/7)",
            AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses = new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "GB",
                    Count = 1,
                    Share = 1d,
                    WeightedCount = 7,
                    WeightedShare = 1d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB },
                    AlgorithmFamilies = new[] { "LoadB" },
                    MethodKeys = new[] { "LoadB:5" },
                    LegacyMethodNames = new[] { "B法负载试验" },
                    SettingsMethodNames = new[] { "B法负载试验" },
                    AnchorKeys = new[] { "gb-temperature-branch" },
                    SuggestedNextStepPriorities = new[] { "blocking" },
                    SuggestedNextStepFocuses = new[] { "热态分支" },
                    Summary = "family=LoadB; cross-plan primary field GB appears in 1/1 plans (100pp), weighted 7/7 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=热态分支; priorities=blocking"
                }
            },
            AlgorithmFamilyRequiredResultPrimaryFieldSummary = "algorithm-family required-result primary fields top 1/1: Pcu2=1 (100pp, weighted 7/7)",
            AlgorithmFamilyRequiredResultPrimaryFieldFocuses = new[]
            {
                new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = "Pcu2",
                    Count = 1,
                    Share = 1d,
                    WeightedCount = 7,
                    WeightedShare = 1d,
                    CanonicalCodes = new[] { MotorYTestMethodCodes.LoadB },
                    AlgorithmFamilies = new[] { "LoadB" },
                    SuggestedNextStepPriorities = new[] { "result-fields" },
                    SuggestedNextStepFocuses = new[] { "结果字段" },
                    Summary = "family=LoadB; cross-plan primary field Pcu2 appears in 1/1 plans (100pp), weighted 7/7 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=结果字段; priorities=result-fields"
                }
            },
            LegacyDecisionAnchorResolutionSummary = "decision anchor resolutions resolved 0/0 (100pp)",
            LegacyAlgorithmInputReadinessSummary = "legacy algorithm inputs ready",
            SelectedMethodSummary = "推荐方法切到 dominant",
            BaselineDominantComparisonSummary = "dominant 明显领先 baseline"
        };

        var formatter = typeof(TestBootstrap).GetMethod("FormatMethodAdaptationPlanSnapshot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?? throw new InvalidOperationException("TestBootstrap formatter not found.");
        var formatted = formatter.Invoke(null, new object[] { plan }) as string
            ?? throw new InvalidOperationException("TestBootstrap formatter returned null.");

        if (!formatted.Contains("anchor-family=GB:1:100.0 %:weighted=7:100.0 %:methods=:method-keys=LoadB:5:profiles=:legacy-methods=B法负载试验:settings-methods=B法负载试验:MotorY.LoadB:families=LoadB:blocking:summary=algorithm-family decision-anchor primary fields top 1/1: GB=1 (100pp, weighted 7/7)", StringComparison.Ordinal)
            || !formatted.Contains("result-family=Pcu2:1:100.0 %:weighted=7:100.0 %:methods=:method-keys=:profiles=:legacy-methods=:settings-methods=:MotorY.LoadB:families=LoadB:result-fields:summary=algorithm-family required-result primary fields top 1/1: Pcu2=1 (100pp, weighted 7/7)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap algorithm-family plan formatting smoke test failed. actual='{formatted}'");
        }
    }

    private static void ShouldBuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses()
    {
        var focuses = MotorYPrimaryFieldFocusFactory.BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(new[]
        {
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.NoLoad,
                AlgorithmFamily = "NoLoad",
                SelectedCount = 3,
                DecisionAnchorPrimaryFieldDistributions = new[]
                {
                    new MotorYDecisionAnchorPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "Pfw",
                        Count = 1,
                        Share = 1d,
                        AnchorKeys = new[] { "pfw-fit-window" },
                        SuggestedNextStepFocuses = new[] { "空载风摩耗拟合窗口" },
                        SuggestedNextStepPriorities = new[] { "blocking" }
                    }
                }
            },
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.LoadB,
                AlgorithmFamily = "LoadB",
                SelectedCount = 7,
                DecisionAnchorPrimaryFieldDistributions = new[]
                {
                    new MotorYDecisionAnchorPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "Ps",
                        Count = 1,
                        Share = 1d,
                        AnchorKeys = new[] { "ps-iteration" },
                        SuggestedNextStepFocuses = new[] { "B法 Ps 非负迭代收敛字段" },
                        SuggestedNextStepPriorities = new[] { "blocking" }
                    }
                }
            }
        });

        var noLoad = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var loadB = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Ps", StringComparison.Ordinal));

        if (focuses.Count != 2
            || noLoad is null
            || loadB is null
            || !noLoad.AlgorithmFamilies.SequenceEqual(new[] { "NoLoad" }, StringComparer.Ordinal)
            || !loadB.AlgorithmFamilies.SequenceEqual(new[] { "LoadB" }, StringComparer.Ordinal)
            || noLoad.WeightedCount != 3
            || loadB.WeightedCount != 7
            || !noLoad.Summary.StartsWith("family=NoLoad; cross-plan primary field Pfw appears in 1/1 plans (100pp), weighted 3/3 selected samples (100pp)", StringComparison.Ordinal)
            || !loadB.Summary.StartsWith("family=LoadB; cross-plan primary field Ps appears in 1/1 plans (100pp), weighted 7/7 selected samples (100pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap algorithm-family decision-anchor focus smoke test failed. actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.AlgorithmFamilies)}:{x.WeightedCount}:{x.WeightedShare:P1}:{x.Summary}"))}]");
        }
    }

    private static void ShouldBuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses()
    {
        var focuses = MotorYPrimaryFieldFocusFactory.BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(new[]
        {
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.NoLoad,
                AlgorithmFamily = "NoLoad",
                SelectedCount = 4,
                RequiredResultPrimaryFieldDistributions = new[]
                {
                    new MotorYRequiredResultPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "CoefficientOfPfe",
                        Count = 2,
                        Share = 1d,
                        BucketKeys = new[] { "result-fields" },
                        DisplayNames = new[] { "结果字段" }
                    }
                }
            },
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.LoadA,
                AlgorithmFamily = "LoadA",
                SelectedCount = 6,
                RequiredResultPrimaryFieldDistributions = new[]
                {
                    new MotorYRequiredResultPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "Pcu2",
                        Count = 1,
                        Share = 1d,
                        BucketKeys = new[] { "result-fields" },
                        DisplayNames = new[] { "结果字段" }
                    }
                }
            }
        });

        var noLoad = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var loadA = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal));

        if (focuses.Count != 2
            || noLoad is null
            || loadA is null
            || !noLoad.AlgorithmFamilies.SequenceEqual(new[] { "NoLoad" }, StringComparer.Ordinal)
            || !loadA.AlgorithmFamilies.SequenceEqual(new[] { "LoadA" }, StringComparer.Ordinal)
            || noLoad.WeightedCount != 4
            || loadA.WeightedCount != 6
            || !noLoad.Summary.StartsWith("family=NoLoad; cross-plan primary field CoefficientOfPfe appears in 1/1 plans (100pp), weighted 4/4 selected samples (100pp)", StringComparison.Ordinal)
            || !loadA.Summary.StartsWith("family=LoadA; cross-plan primary field Pcu2 appears in 1/1 plans (100pp), weighted 6/6 selected samples (100pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap algorithm-family required-result focus smoke test failed. actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.AlgorithmFamilies)}:{x.WeightedCount}:{x.WeightedShare:P1}:{x.Summary}"))}]");
        }
    }

    private static void ShouldBuildVariantKindDecisionAnchorPrimaryFieldFocuses()
    {
        var focuses = MotorYPrimaryFieldFocusFactory.BuildVariantKindDecisionAnchorPrimaryFieldFocuses(new[]
        {
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.NoLoad,
                SelectedCount = 3,
                SelectedRoute = new MotorYLegacyAlgorithmRoute { CanonicalCode = MotorYTestMethodCodes.NoLoad, MethodValue = 0, VariantKind = MotorYLegacyVariantKinds.Baseline, AlgorithmFamily = MotorYLegacyAlgorithmFamilies.NoLoad },
                DecisionAnchorPrimaryFieldDistributions = new[]
                {
                    new MotorYDecisionAnchorPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "Pfw",
                        Count = 1, Share = 1d, AnchorKeys = new[] { "pfw-fit-window" }, SuggestedNextStepFocuses = new[] { "空载风摩耗拟合窗口" }, SuggestedNextStepPriorities = new[] { "blocking" }
                    }
                }
            }
        });

        var baseline = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        if (focuses.Count != 1
            || baseline is null
            || !baseline.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal)
            || baseline.WeightedCount != 3
            || !baseline.Summary.StartsWith("variant=baseline; cross-plan primary field Pfw appears in 1/1 plans (100pp), weighted 3/3 selected samples (100pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap variant-kind decision-anchor focus smoke test failed. actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.VariantKinds)}:{x.WeightedCount}:{x.WeightedShare:P1}:{x.Summary}"))}]");
        }
    }

    private static void ShouldBuildVariantKindRequiredResultPrimaryFieldFocuses()
    {
        var focuses = MotorYPrimaryFieldFocusFactory.BuildVariantKindRequiredResultPrimaryFieldFocuses(new[]
        {
            new MotorYMethodAdaptationPlanSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.LoadA,
                SelectedCount = 6,
                SelectedRoute = new MotorYLegacyAlgorithmRoute { CanonicalCode = MotorYTestMethodCodes.LoadA, MethodValue = 4, VariantKind = MotorYLegacyVariantKinds.Baseline, AlgorithmFamily = MotorYLegacyAlgorithmFamilies.LoadA },
                RequiredResultPrimaryFieldDistributions = new[]
                {
                    new MotorYRequiredResultPrimaryFieldDistributionSnapshot
                    {
                        PrimaryField = "Pcu2",
                        Count = 1, Share = 1d, BucketKeys = new[] { "result-fields" }, DisplayNames = new[] { "结果字段" }
                    }
                }
            }
        });

        var baseline = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal));
        if (focuses.Count != 1
            || baseline is null
            || !baseline.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal)
            || baseline.WeightedCount != 6
            || !baseline.Summary.StartsWith("variant=baseline; cross-plan primary field Pcu2 appears in 1/1 plans (100pp), weighted 6/6 selected samples (100pp)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestBootstrap variant-kind required-result focus smoke test failed. actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.VariantKinds)}:{x.WeightedCount}:{x.WeightedShare:P1}:{x.Summary}"))}]");
        }
    }

}
