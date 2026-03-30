using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyAlgorithmDependencyCatalogSmokeTests
{
    public static void Run()
    {
        var expected = new[]
        {
            (MotorYTestMethodCodes.DcResistance, false, Array.Empty<string>(), new[] { "Ruv", "Rvw", "Rwu", "R1", "R1c", "θ1c" }, Array.Empty<string>(), new[] { "R1", "R1c", "θ1c" }, new[] { "R1", "R1c", "θ1c" }),
            (MotorYTestMethodCodes.NoLoad, false, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" }, Array.Empty<string>(), new[] { "I0", "ΔI0", "P0", "Pcu", "Pfw", "Pfe", "CoefficientOfPfe" }, new[] { "R0", "θ0", "Pcon", "P0cu1", "Pfw", "Pfe", "CoefficientOfPfe" }),
            (MotorYTestMethodCodes.HeatRun, true, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" }, new[] { "GB" }, new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" }, new[] { "firstSecondsInterval", "Rw", "Rn", "Rws", "θw", "θs", "θb" }),
            (MotorYTestMethodCodes.LoadA, false, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" }, Array.Empty<string>(), new[] { "Pcu1", "Pcu2", "ResultDataList", "η" }, new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "P2x", "η" }),
            (MotorYTestMethodCodes.LoadB, true, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" }, new[] { "GB" }, new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "Pl", "A", "B", "R", "Ps", "cuC", "θs" }),
            (MotorYTestMethodCodes.LockedRotor, false, new[] { MotorYTestMethodCodes.NoLoad }, new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" }, Array.Empty<string>(), new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" }, new[] { "θ1s", "R", "Pkcu1", "Pfe", "ns", "Tk", "Ikn", "Pkn", "Tkn" })
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

            if (!profile.RequiredIntermediateResultFields.SequenceEqual(row.Item7, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: intermediate result fields mismatch for {row.Item1}.");
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

            if (profile.LegacyDecisionAnchorRequiredFields.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor required fields missing for {row.Item1}.");
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

        var dcResistanceDecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.DcResistance,
            new[] { "R1c", "θ1c" },
            null);

        if (dcResistanceDecisionAnchorObservationRules.Count != 2
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.CoveredByObservedPayload)
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.ObservedPayloadFields.SequenceEqual(new[] { "R1", "R1c", "θ1c" }, StringComparer.Ordinal))
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.MissingPayloadFields.Count == 0)
            || !dcResistanceDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "cold-baseline-ready", StringComparison.Ordinal)
                && string.Equals(rule.Summary, "decision-anchor-observation:cold-baseline-ready covered by observed payload fields 'R1', 'R1c', 'θ1c'", StringComparison.Ordinal))
            || !dcResistanceDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "downstream-ready", StringComparison.Ordinal)
                && string.Equals(rule.Summary, "decision-anchor-observation:downstream-ready covered by observed payload fields 'R1', 'R1c', 'θ1c'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for DcResistance. actual=[{string.Join(" | ", dcResistanceDecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var heatRunDecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.HeatRun,
            new[] { "HotStateType", "GB", "Pn", "Rw", "θb", "θw" },
            null);

        if (heatRunDecisionAnchorObservationRules.Count != 3
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "first-seconds-interval", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "Pn" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "hot-state-branch", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "HotStateType" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "gb-temperature-branch", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "GB", "Rn", "θb", "θs", "θw" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:gb-temperature-branch covered by observed payload fields 'GB', 'Rn', 'θb', 'θs', 'θw'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for HeatRun. actual=[{string.Join(" | ", heatRunDecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var loadADecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.LoadA,
            new[] { "CoefficientOfPfe", "Pfw", "ResultDataList", "θa" },
            null);

        if (loadADecisionAnchorObservationRules.Count != 3
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "upstream-ready", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "CoefficientOfPfe", "Pfw", "θa" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "rated-load-fit-grid", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "payload-rated-quantity-ready", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:payload-rated-quantity-ready covered by observed payload fields 'Pcu1', 'Pcu2', 'η'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for LoadA. actual=[{string.Join(" | ", loadADecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var loadBDecisionAnchorObservationGaps = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationGaps(
            MotorYTestMethodCodes.LoadB,
            new[] { "A", "B", "GB", "R", "ResultDataList", "θb", "θs", "θw" },
            null);

        if (loadBDecisionAnchorObservationGaps.Count != 3
            || loadBDecisionAnchorObservationGaps.Count(gap => gap.CoveredByObservedPayload) != 2
            || loadBDecisionAnchorObservationGaps.Count(gap => !gap.CoveredByObservedPayload) != 1
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:gb-ratios-branch", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "GB", "θs" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:correlation-refit", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "A", "B", "R" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:ps-iteration", StringComparison.Ordinal)
                && !gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.SequenceEqual(new[] { "Ps" }, StringComparer.Ordinal)
                && string.Equals(gap.Summary, "decision-anchor-observation:ps-iteration missing observed payload fields 'Ps'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation rule projection mismatch for LoadB. actual=[{string.Join(" | ", loadBDecisionAnchorObservationGaps.Select(gap => $"{gap.SignalOrRule}:{gap.CoveredByObservedPayload}:{string.Join(",", gap.ObservedPayloadFields)}:{string.Join(",", gap.MissingPayloadFields)}"))}]");
        }

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
            || !loadBPlan.RequiredIntermediateResultFields.SequenceEqual(new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "Pl", "A", "B", "R", "Ps", "cuC", "θs" }, StringComparer.Ordinal)
            || loadBPlan.CoveredRequiredIntermediateResultFieldCount != 5
            || loadBPlan.MissingRequiredIntermediateResultFieldCount != 11
            || !string.Equals(loadBPlan.RequiredIntermediateResultFieldCoverageSummary, $"result required fields covered 5/16 (31pp); missing: R1t, Pcu1t, Nst, St, Ub, Pcu2t, Tx, P2tx, Pl, Ps, cuC", StringComparison.Ordinal)
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
            || loadBPlan.LegacyDecisionAnchorsObservedPayloadFields.Count != 8
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadFields.SequenceEqual(new[] { "A", "B", "GB", "R", "ResultDataList", "θb", "θs", "θw" }, StringComparer.Ordinal)
            || loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Count != 9
            || !loadBPlan.LegacyDecisionAnchorsBackedByObservedPayload
            || loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Count(gap => gap.CoveredByObservedPayload) != 8
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.All(gap => gap.RequiredPayloadFields.Count >= 1)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:A", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "A" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0
                && string.Equals(gap.Summary, "decision-anchor:A covered by observed payload field 'A'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:GB", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0
                && string.Equals(gap.Summary, "decision-anchor:GB covered by observed payload field 'GB'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:B", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "B" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:R", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "R" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:ResultDataList", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:θb", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "θb" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:θs", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "θs" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:θw", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "θw" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor:Ps", StringComparison.Ordinal)
                && !gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.Count == 0
                && gap.MissingPayloadFields.SequenceEqual(new[] { "Ps" }, StringComparer.Ordinal)
                && string.Equals(gap.Summary, "decision-anchor:Ps missing observed payload field 'Ps'", StringComparison.Ordinal))
            || loadBPlan.LegacyDecisionAnchorObservationRules.Count != 3
            || loadBPlan.CoveredLegacyDecisionAnchorObservationRuleCount != 2
            || loadBPlan.MissingLegacyDecisionAnchorObservationRuleCount != 1
            || Math.Abs(loadBPlan.LegacyDecisionAnchorObservationRuleCoverageRatio - (2d / 3d)) > 0.0001d
            || loadBPlan.LegacyDecisionAnchorObservationRuleCoveragePercentagePoints != 67
            || !string.Equals(loadBPlan.LegacyDecisionAnchorObservationRuleSummary, "decision anchor observation rules covered 2/3 (67pp); missing: ps-iteration", StringComparison.Ordinal)
            || !loadBPlan.LegacyDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "gb-ratios-branch", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.RequiredPayloadFields.SequenceEqual(new[] { "GB", "θs" }, StringComparer.Ordinal)
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "GB", "θs" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:gb-ratios-branch covered by observed payload fields 'GB', 'θs'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "correlation-refit", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.RequiredPayloadFields.SequenceEqual(new[] { "A", "B", "R" }, StringComparer.Ordinal)
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "A", "B", "R" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:correlation-refit covered by observed payload fields 'A', 'B', 'R'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "ps-iteration", StringComparison.Ordinal)
                && !rule.CoveredByObservedPayload
                && rule.RequiredPayloadFields.SequenceEqual(new[] { "ResultDataList", "Ps" }, StringComparer.Ordinal)
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.SequenceEqual(new[] { "Ps" }, StringComparer.Ordinal)
                && string.Equals(rule.Summary, "decision-anchor-observation:ps-iteration missing observed payload fields 'Ps'", StringComparison.Ordinal))
            || loadBPlan.LegacyDecisionAnchorResolutions.Count != 4
            || loadBPlan.ResolvedLegacyDecisionAnchorCount != 0
            || loadBPlan.PartialLegacyDecisionAnchorCount != 2
            || loadBPlan.MissingLegacyDecisionAnchorResolutionCount != 2
            || Math.Abs(loadBPlan.LegacyDecisionAnchorResolutionCoverageRatio) > 0.0001d
            || loadBPlan.LegacyDecisionAnchorResolutionCoveragePercentagePoints != 0
            || !string.Equals(loadBPlan.LegacyDecisionAnchorResolutionSummary, "decision anchor resolutions resolved 0/4 (0pp); partial=2; missing=2; unresolved: correlation-refit:partial, gb-ratios-branch:partial, ps-iteration:missing, thermal-carryover:missing", StringComparison.Ordinal)
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "gb-ratios-branch", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && resolution.PartiallyResolvedByObservedPayload
                && resolution.RequiredPayloadFields.SequenceEqual(new[] { "GB", "θs", "ratios" }, StringComparer.Ordinal)
                && resolution.ObservedPayloadFields.SequenceEqual(new[] { "GB", "θs" }, StringComparer.Ordinal)
                && resolution.MissingPayloadFields.SequenceEqual(new[] { "ratios" }, StringComparer.Ordinal)
                && Math.Abs(resolution.CoverageRatio - (2d / 3d)) < 0.0001d
                && resolution.CoveragePercentagePoints == 67
                && string.Equals(resolution.ResolutionStage, "partial", StringComparison.Ordinal)
                && string.Equals(resolution.Summary, "decision-anchor-resolution:gb-ratios-branch partial 2/3; missing 'ratios'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "correlation-refit", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && resolution.PartiallyResolvedByObservedPayload
                && resolution.RequiredPayloadFields.SequenceEqual(new[] { "A", "B", "R", "bad-point-refit" }, StringComparer.Ordinal)
                && resolution.ObservedPayloadFields.SequenceEqual(new[] { "A", "B", "R" }, StringComparer.Ordinal)
                && resolution.MissingPayloadFields.SequenceEqual(new[] { "bad-point-refit" }, StringComparer.Ordinal)
                && Math.Abs(resolution.CoverageRatio - 0.75d) < 0.0001d
                && resolution.CoveragePercentagePoints == 75
                && string.Equals(resolution.ResolutionStage, "partial", StringComparison.Ordinal)
                && string.Equals(resolution.Summary, "decision-anchor-resolution:correlation-refit partial 3/4; missing 'bad-point-refit'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "ps-iteration", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && !resolution.PartiallyResolvedByObservedPayload
                && resolution.RequiredPayloadFields.SequenceEqual(new[] { "ResultDataList", "Ps", "cuC" }, StringComparer.Ordinal)
                && resolution.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && resolution.MissingPayloadFields.SequenceEqual(new[] { "Ps", "cuC" }, StringComparer.Ordinal)
                && Math.Abs(resolution.CoverageRatio - (1d / 3d)) < 0.0001d
                && resolution.CoveragePercentagePoints == 33
                && string.Equals(resolution.ResolutionStage, "missing", StringComparison.Ordinal)
                && string.Equals(resolution.Summary, "decision-anchor-resolution:ps-iteration missing observed payload fields 'Ps', 'cuC'", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "thermal-carryover", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && !resolution.PartiallyResolvedByObservedPayload
                && resolution.RequiredPayloadFields.SequenceEqual(new[] { "θw", "θb" }, StringComparer.Ordinal)
                && resolution.ObservedPayloadFields.Count == 0
                && resolution.MissingPayloadFields.SequenceEqual(new[] { "θb", "θw" }, StringComparer.Ordinal)
                && Math.Abs(resolution.CoverageRatio) < 0.0001d
                && resolution.CoveragePercentagePoints == 0
                && string.Equals(resolution.ResolutionStage, "missing", StringComparison.Ordinal)
                && string.Equals(resolution.Summary, "decision-anchor-resolution:thermal-carryover missing observed payload fields 'θb', 'θw'", StringComparison.Ordinal))
            || !string.Equals(loadBPlan.FormulaSignalSummary, "formula signals covered 0/3 (0pp); missing: 先逐点计算 R1t/Pcu1t/Nst/St/Ub/Pfe/Pcu2t/Tx/P2tx/Pl，再用 Tx²-Pl 相关关系求附加损耗系数 A/B/R, 当 R<0.95 时执行一次删除坏点，再重新拟合 A/B/R, 依据 GB 版本切换 θs 与 ratios 口径，并生成 ResultDataList", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyAlgorithmRuleSummary, "legacy algorithm rules covered 0/3 (0pp); missing: GB1032_2012/TB_朝阳电机 使用 1.5/1.25/1/0.75/0.5/0.25 负载点，GB1032_2023 使用 1.25/1.15/1/0.75/0.5/0.25, 2012/2023 国标分支以 θw+25-θb 推导 θs，朝阳电机分支按每个负载点 θ1t/θa 单点计算 θs, 结果区会循环下调铜耗系数 cuC，直到所有负载点附加损耗 Ps 非负", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyDecisionAnchorSummary, "legacy decision anchors covered 0/3 (0pp); missing: GB 版本决定 ratios 负载点集与 θs 计算分支，B 法不能脱离 ratedParams.GB 运行, 当相关系数 R<0.95 时需先删坏点再重新拟合 A/B/R, 结果区会从 cuC=1 开始逐步下调，直到所有 Ps 非负，说明旧算法存在迭代收敛决策", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: stp.db adaptation plan dependency projection mismatch for LoadB. actual decisionAnchorCovered={loadBPlan.CoveredLegacyDecisionAnchorCount}, missing={loadBPlan.MissingLegacyDecisionAnchorCount}, ratio={loadBPlan.LegacyDecisionAnchorCoverageRatio}, pp={loadBPlan.LegacyDecisionAnchorCoveragePercentagePoints}, backed={loadBPlan.LegacyDecisionAnchorsBackedByObservedPayload}, gaps=[{string.Join(" | ", loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Select(gap => $"{gap.SignalOrRule}:{gap.CoveredByObservedPayload}:{string.Join(",", gap.ObservedPayloadFields)}:{string.Join(",", gap.MissingPayloadFields)}"))}], summary='{loadBPlan.LegacyDecisionAnchorSummary}'");
        }

        var bucketMap = loadBPlan.DependencyBuckets.ToDictionary(x => x.BucketKey, StringComparer.Ordinal);
        if (bucketMap.Count != 12
            || !bucketMap.TryGetValue("upstream", out var upstreamBucket)
            || upstreamBucket.CoveredCount != 2
            || upstreamBucket.MissingCount != 0
            || !bucketMap.TryGetValue("rated-params", out var ratedBucket)
            || ratedBucket.CoveredCount != 1
            || ratedBucket.MissingCount != 0
            || !bucketMap.TryGetValue("intermediate-result-fields", out var intermediateBucket)
            || intermediateBucket.RequiredCount != 16
            || intermediateBucket.CoveredCount != 5
            || intermediateBucket.MissingCount != 11
            || !intermediateBucket.MissingItems.SequenceEqual(new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pcu2t", "Tx", "P2tx", "Pl", "Ps", "cuC" }, StringComparer.Ordinal)
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
            || decisionAnchorBucket.MissingCount != 3
            || !bucketMap.TryGetValue("legacy-decision-anchor-resolutions", out var decisionResolutionBucket)
            || decisionResolutionBucket.RequiredCount != 4
            || decisionResolutionBucket.CoveredCount != 0
            || decisionResolutionBucket.MissingCount != 4
            || !decisionResolutionBucket.MissingItems.SequenceEqual(new[] { "correlation-refit:partial", "gb-ratios-branch:partial", "ps-iteration:missing", "thermal-carryover:missing" }, StringComparer.Ordinal)
            || !string.Equals(decisionResolutionBucket.Summary, "decision anchor resolutions covered 0/4 (0pp); partial=2; missing=2; unresolved: correlation-refit:partial, gb-ratios-branch:partial, ps-iteration:missing, thermal-carryover:missing", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: dependency bucket projection mismatch for LoadB.");
        }
    }
}
