using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyAlgorithmDependencyCatalogSmokeTests
{
    public static void Run()
    {
        var expected = new[]
        {
            (MotorYTestMethodCodes.DcResistance, false, Array.Empty<string>(), new[] { "Ruv", "Rvw", "Rwu", "R1", "θ1c" }, Array.Empty<string>(), new[] { "R1", "θ1c" }),
            (MotorYTestMethodCodes.NoLoad, false, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" }, Array.Empty<string>(), new[] { "I0", "ΔI0", "P0", "Pcu", "Pfw", "Pfe", "CoefficientOfPfe" }),
            (MotorYTestMethodCodes.HeatRun, true, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" }, new[] { "GB" }, new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" }),
            (MotorYTestMethodCodes.LoadA, false, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" }, Array.Empty<string>(), new[] { "Pcu1", "Pcu2", "ResultDataList", "η" }),
            (MotorYTestMethodCodes.LoadB, true, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" }, new[] { "GB" }, new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }),
            (MotorYTestMethodCodes.LockedRotor, false, new[] { MotorYTestMethodCodes.NoLoad }, new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" }, Array.Empty<string>(), new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" })
        };

        foreach (var row in expected)
        {
            var profile = MotorYLegacyAlgorithmDependencyCatalog.TryGet(row.Item1)
                ?? throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: missing profile for {row.Item1}.");

            if (profile.RequiresRatedParams != row.Item2)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: RequiresRatedParams mismatch for {row.Item1}.");
            }

            if (!profile.UpstreamCanonicalCodes.SequenceEqual(row.Item3, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: upstream mismatch for {row.Item1}.");
            }

            if (!profile.RequiredPayloadFields.SequenceEqual(row.Item4, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: payload fields mismatch for {row.Item1}.");
            }

            if (!profile.RequiredRatedParamFields.SequenceEqual(row.Item5, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: rated param fields mismatch for {row.Item1}.");
            }

            if (!profile.RequiredResultFields.SequenceEqual(row.Item6, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: result fields mismatch for {row.Item1}.");
            }

            if (profile.RequiredStructuredPayloadSignals.Count == 0
                || profile.RequiredStructuredResultSignals.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: structured signal dependency missing for {row.Item1}.");
            }

            if (string.IsNullOrWhiteSpace(profile.Notes)
                || string.IsNullOrWhiteSpace(profile.AlgorithmEntry)
                || profile.FormulaSignals.Count == 0
                || profile.LegacyAlgorithmRules.Count == 0
                || profile.LegacyDecisionAnchors.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: notes/algorithm entry/formula rules/decision anchors missing for {row.Item1}.");
            }
        }

        var decision = new MotorYMethodDecisionSnapshot
        {
            CanonicalCode = MotorYTestMethodCodes.LoadB,
            TotalCount = 10,
            BaselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
            BaselineCount = 4,
            DominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 51),
            DominantCount = 6,
            RecommendedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
            RecommendedStrategy = "baseline",
            ShouldPrioritizeDominantOverBaseline = false,
            DominantShare = 0.6,
            BaselineShare = 0.4,
            DominantOverrideThreshold = 0.7,
            DominantLeadCount = 2,
            DominantLeadPercentagePoints = 20,
            RecommendationReason = "test",
            RecommendedMethodSummary = "test",
            BaselineDominantComparisonSummary = "test",
            Distributions =
            [
                new MotorYMethodDistributionSnapshot
                {
                    MethodValue = 5,
                    Count = 4,
                    Share = 0.4,
                    Route = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5)
                },
                new MotorYMethodDistributionSnapshot
                {
                    MethodValue = 51,
                    Count = 6,
                    Share = 0.6,
                    Route = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 51)
                }
            ]
        };

        var contract = MotorYMethodAdaptationPlanContractMapper.Map(decision, route => route is null
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
            });

                var snapshotService = new StpDbSnapshotQueryService();
        var plans = snapshotService.ListMotorYMethodAdaptationPlans();
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: missing LoadB adaptation plan from stp.db snapshot.");


        if (!loadBPlan.RequiresRatedParams
            || !string.Equals(loadBPlan.RecommendedLegacyCode, "B法负载试验", StringComparison.Ordinal)
            || loadBPlan.RecommendedLegacyCodeCount != 265
            || Math.Abs(loadBPlan.RecommendedLegacyCodeShare - 1d) > 0.0001d
            || loadBPlan.LegacyCodeDistributions.Count != 1
            || !string.Equals(loadBPlan.LegacyCodeSelectionSummary, "recommended legacy code 'B法负载试验' for MotorY.LoadB (265/265, 100pp)", StringComparison.Ordinal)
            || !loadBPlan.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
            || !loadBPlan.RequiredResultFields.SequenceEqual(new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, StringComparer.Ordinal)
            || !loadBPlan.UpstreamDependenciesSatisfied
            || !string.Equals(loadBPlan.UpstreamDependencySummary, "upstream dependencies satisfied (MotorY.NoLoad + MotorY.HeatRun); observed 2/2 required upstream codes", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.RawDataSignalCoverageSummary, $"raw data signals covered 7/8 (88pp); raw samples={loadBPlan.RawDataSampleCount}; missing: θa; observed: Frequency, I1, Nt, P1t, Tt, U, θ1t", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyAlgorithmRulesObservedPayloadSummary, "legacy algorithm rule observed payload fields observed 8/9 (89pp); missing: Ps; observed: A, B, GB, R, ResultDataList, θb, θs, θw", StringComparison.Ordinal)
            || !loadBPlan.LegacyDecisionAnchors.SequenceEqual(new[]
            {
                "GB 版本决定 ratios 负载点集与 θs 计算分支，B 法不能脱离 ratedParams.GB 运行",
                "当相关系数 R<0.95 时需先删坏点再重新拟合 A/B/R",
                "结果区会从 cuC=1 开始逐步下调，直到所有 Ps 非负，说明旧算法存在迭代收敛决策"
            }, StringComparer.Ordinal)
            || loadBPlan.CoveredLegacyDecisionAnchorCount != 0
            || loadBPlan.MissingLegacyDecisionAnchorCount != 3
            || Math.Abs(loadBPlan.LegacyDecisionAnchorCoverageRatio) > 0.0001d
            || loadBPlan.LegacyDecisionAnchorCoveragePercentagePoints != 0
            || !string.Equals(loadBPlan.LegacyDecisionAnchorsObservedPayloadSummary, "legacy decision anchor observed payload fields observed 8/9 (89pp); missing: Ps; observed: A, B, GB, R, ResultDataList, θb, θs, θw", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.FormulaSignalSummary, "formula signals covered 0/3 (0pp); missing: 先逐点计算 R1t/Pcu1t/Nst/St/Ub/Pfe/Pcu2t/Tx/P2tx/Pl，再用 Tx²-Pl 相关关系求附加损耗系数 A/B/R, 当 R<0.95 时执行一次删除坏点，再重新拟合 A/B/R, 依据 GB 版本切换 θs 与 ratios 口径，并生成 ResultDataList", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyAlgorithmRuleSummary, "legacy algorithm rules covered 0/3 (0pp); missing: GB1032_2012/TB_朝阳电机 使用 1.5/1.25/1/0.75/0.5/0.25 负载点，GB1032_2023 使用 1.25/1.15/1/0.75/0.5/0.25, 2012/2023 国标分支以 θw+25-θb 推导 θs，朝阳电机分支按每个负载点 θ1t/θa 单点计算 θs, 结果区会循环下调铜耗系数 cuC，直到所有负载点附加损耗 Ps 非负", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyDecisionAnchorSummary, "legacy decision anchors covered 0/3 (0pp); missing: GB 版本决定 ratios 负载点集与 θs 计算分支，B 法不能脱离 ratedParams.GB 运行, 当相关系数 R<0.95 时需先删坏点再重新拟合 A/B/R, 结果区会从 cuC=1 开始逐步下调，直到所有 Ps 非负，说明旧算法存在迭代收敛决策", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: stp.db adaptation plan dependency projection mismatch for LoadB.");
        }

        var bucketMap = loadBPlan.DependencyBuckets.ToDictionary(x => x.BucketKey, StringComparer.Ordinal);
        if (bucketMap.Count != 10
            || !bucketMap.TryGetValue("upstream", out var upstreamBucket)
            || upstreamBucket.CoveredCount != 2
            || upstreamBucket.MissingCount != 0
            || !bucketMap.TryGetValue("rated-params", out var ratedBucket)
            || ratedBucket.CoveredCount != 1
            || ratedBucket.MissingCount != 0
            || !bucketMap.TryGetValue("raw-data-signals", out var rawBucket)
            || rawBucket.RequiredCount != 8
            || rawBucket.CoveredCount != 7
            || rawBucket.MissingCount != 1
            || !rawBucket.MissingItems.SequenceEqual(new[] { "θa" }, StringComparer.Ordinal)
            || !bucketMap.TryGetValue("formula-signals", out var formulaBucket)
            || formulaBucket.RequiredCount != 3
            || formulaBucket.CoveredCount != 0
            || formulaBucket.MissingCount != 3
            || !bucketMap.TryGetValue("legacy-rules", out var ruleBucket)
            || ruleBucket.RequiredCount != 3
            || ruleBucket.CoveredCount != 0
            || ruleBucket.MissingCount != 3
            || !bucketMap.TryGetValue("legacy-decision-anchors", out var decisionAnchorBucket)
            || decisionAnchorBucket.RequiredCount != 3
            || decisionAnchorBucket.CoveredCount != 0
            || decisionAnchorBucket.MissingCount != 3)
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: dependency bucket projection mismatch for LoadB.");
        }
    }
}
