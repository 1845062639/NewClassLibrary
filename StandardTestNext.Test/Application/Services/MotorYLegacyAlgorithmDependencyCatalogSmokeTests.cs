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

            if (string.IsNullOrWhiteSpace(profile.Notes)
                || string.IsNullOrWhiteSpace(profile.AlgorithmEntry)
                || profile.FormulaSignals.Count == 0
                || profile.LegacyAlgorithmRules.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: notes/algorithm entry/formula rules missing for {row.Item1}.");
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

        if (!contract.RequiresRatedParams
            || !contract.UpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, StringComparer.Ordinal)
            || contract.ObservedUpstreamCanonicalCodeCount != 0
            || contract.ObservedUpstreamCanonicalCodes.Count != 0
            || !contract.MissingUpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, StringComparer.Ordinal)
            || contract.UpstreamDependenciesSatisfied
            || !string.Equals(contract.UpstreamDependencySummary, "upstream dependencies missing 2/2: NoLoad, HeatRun; observed 0/2 required upstream codes", StringComparison.Ordinal)
            || !contract.RequiredPayloadFields.Contains("θw", StringComparer.Ordinal)
            || !contract.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
            || !contract.RequiredResultFields.SequenceEqual(new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, StringComparer.Ordinal)
            || contract.CoveredRequiredResultFieldCount != 0
            || contract.MissingRequiredResultFieldCount != 7
            || contract.CoveredRequiredResultFields.Count != 0
            || !contract.MissingRequiredResultFields.SequenceEqual(new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, StringComparer.Ordinal)
            || contract.RequiredResultFieldCoverageRatio != 0d
            || contract.RequiredResultFieldCoveragePercentagePoints != 0
            || !string.Equals(contract.RequiredResultFieldCoverageSummary, "result required fields covered 0/7 (0pp); missing: A, B, R, Pcu1, Pcu2, θs, ResultDataList", StringComparison.Ordinal)
            || contract.SamplePayloadAvailable
            || contract.RequiredPayloadFieldCoverageRatio != 0d
            || contract.RequiredPayloadFieldCoveragePercentagePoints != 0
            || contract.CoveredRequiredPayloadFields.Count != 0
            || contract.RatedParamsAvailable
            || contract.RequiredRatedParamFieldCoverageRatio != 0d
            || contract.RequiredRatedParamFieldCoveragePercentagePoints != 0
            || contract.CoveredRequiredRatedParamFields.Count != 0
            || contract.LegacyAlgorithmInputsReady
            || contract.FormulaSignals.Count == 0
            || contract.CoveredFormulaSignalCount != contract.FormulaSignals.Count
            || contract.MissingFormulaSignalCount != 0
            || !contract.CoveredFormulaSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(contract.FormulaSignals.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || contract.MissingFormulaSignals.Count != 0
            || Math.Abs(contract.FormulaSignalCoverageRatio - 1d) > 0.0001d
            || contract.FormulaSignalCoveragePercentagePoints != 100
            || contract.FormulaSignalsBackedByObservedPayload
            || contract.FormulaSignalsObservedPayloadFields.Count != 0
            || !string.Equals(contract.FormulaSignalsObservedPayloadSummary, "formula signal observed payload fields observed 0/5; observed: none", StringComparison.Ordinal)
            || contract.LegacyAlgorithmRules.Count == 0
            || contract.CoveredLegacyAlgorithmRuleCount != contract.LegacyAlgorithmRules.Count
            || contract.MissingLegacyAlgorithmRuleCount != 0
            || !contract.CoveredLegacyAlgorithmRules.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(contract.LegacyAlgorithmRules.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || contract.MissingLegacyAlgorithmRules.Count != 0
            || Math.Abs(contract.LegacyAlgorithmRuleCoverageRatio - 1d) > 0.0001d
            || contract.LegacyAlgorithmRuleCoveragePercentagePoints != 100
            || contract.LegacyAlgorithmRulesBackedByObservedPayload
            || contract.LegacyAlgorithmRulesObservedPayloadFields.Count != 0
            || !string.Equals(contract.LegacyAlgorithmRulesObservedPayloadSummary, "legacy algorithm rule observed payload fields observed 0/9; observed: none", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(contract.FormulaSignalSummary)
            || string.IsNullOrWhiteSpace(contract.LegacyAlgorithmRuleSummary)
            || !string.Equals(contract.RequiredRatedParamFieldCoverageSummary, "rated param required fields covered 0/1 (0pp); missing: GB", StringComparison.Ordinal)
            || !string.Equals(contract.LegacyAlgorithmInputReadinessSummary, "legacy algorithm inputs incomplete; upstream dependencies missing 2/2: NoLoad, HeatRun; observed 0/2 required upstream codes; payload required fields covered 0/13 (0pp); missing: RawDataList, CoefficientOfPfe, Pfw, R1c, θ1c, θw, θb, PolePairs, Pn, Un, ΔT, K1, K2; rated param required fields covered 0/1 (0pp); missing: GB", StringComparison.Ordinal)
            || !contract.FormulaSignalSummary.Contains("Tx²-Pl", StringComparison.Ordinal)
            || !contract.LegacyAlgorithmRuleSummary.Contains("cuC", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(contract.DependencyNotes))
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: adaptation contract dependency projection mismatch for LoadB.");
        }

        var snapshotService = new StpDbSnapshotQueryService();
        var plans = snapshotService.ListMotorYMethodAdaptationPlans();
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: missing LoadB adaptation plan from stp.db snapshot.");

        if (!loadBPlan.RequiresRatedParams
            || !loadBPlan.UpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, StringComparer.Ordinal)
            || loadBPlan.ObservedUpstreamCanonicalCodeCount != 2
            || !loadBPlan.ObservedUpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.HeatRun, MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || loadBPlan.MissingUpstreamCanonicalCodes.Count != 0
            || !loadBPlan.UpstreamDependenciesSatisfied
            || !string.Equals(loadBPlan.UpstreamDependencySummary, "upstream dependencies satisfied (NoLoad + HeatRun); observed 2/2 required upstream codes", StringComparison.Ordinal)
            || !loadBPlan.RequiredPayloadFields.Contains("θw", StringComparer.Ordinal)
            || !loadBPlan.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
            || !loadBPlan.RequiredResultFields.SequenceEqual(new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, StringComparer.Ordinal)
            || !loadBPlan.SamplePayloadAvailable
            || loadBPlan.CoveredRequiredResultFieldCount != 7
            || loadBPlan.MissingRequiredResultFieldCount != 0
            || !loadBPlan.CoveredRequiredResultFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "A", "B", "Pcu1", "Pcu2", "R", "ResultDataList", "θs" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || loadBPlan.MissingRequiredResultFields.Count != 0
            || Math.Abs(loadBPlan.RequiredResultFieldCoverageRatio - 1d) > 0.0001d
            || loadBPlan.RequiredResultFieldCoveragePercentagePoints != 100
            || !string.Equals(loadBPlan.RequiredResultFieldCoverageSummary, "result required fields covered 7/7 (100pp); missing: none", StringComparison.Ordinal)
            || loadBPlan.CoveredRequiredPayloadFields.Count == 0
            || loadBPlan.RequiredPayloadFieldCoverageRatio <= 0d
            || loadBPlan.RequiredPayloadFieldCoveragePercentagePoints <= 0
            || !loadBPlan.RatedParamsAvailable
            || loadBPlan.CoveredRequiredRatedParamFields.Count == 0
            || loadBPlan.RequiredRatedParamFieldCoverageRatio <= 0d
            || loadBPlan.RequiredRatedParamFieldCoveragePercentagePoints <= 0
            || !loadBPlan.LegacyAlgorithmInputsReady
            || loadBPlan.FormulaSignals.Count == 0
            || loadBPlan.CoveredFormulaSignalCount != loadBPlan.FormulaSignals.Count
            || loadBPlan.MissingFormulaSignalCount != 0
            || !loadBPlan.CoveredFormulaSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(loadBPlan.FormulaSignals.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || loadBPlan.MissingFormulaSignals.Count != 0
            || Math.Abs(loadBPlan.FormulaSignalCoverageRatio - 1d) > 0.0001d
            || loadBPlan.FormulaSignalCoveragePercentagePoints != 100
            || !loadBPlan.FormulaSignalsBackedByObservedPayload
            || !loadBPlan.FormulaSignalsObservedPayloadFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "A", "B", "R", "ResultDataList", "θs" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || !string.Equals(loadBPlan.FormulaSignalsObservedPayloadSummary, "formula signal observed payload fields observed 5/5; observed: A, B, R, ResultDataList, θs", StringComparison.Ordinal)
            || loadBPlan.LegacyAlgorithmRules.Count == 0
            || loadBPlan.CoveredLegacyAlgorithmRuleCount != loadBPlan.LegacyAlgorithmRules.Count
            || loadBPlan.MissingLegacyAlgorithmRuleCount != 0
            || !loadBPlan.CoveredLegacyAlgorithmRules.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(loadBPlan.LegacyAlgorithmRules.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || loadBPlan.MissingLegacyAlgorithmRules.Count != 0
            || Math.Abs(loadBPlan.LegacyAlgorithmRuleCoverageRatio - 1d) > 0.0001d
            || loadBPlan.LegacyAlgorithmRuleCoveragePercentagePoints != 100
            || !loadBPlan.LegacyAlgorithmRulesBackedByObservedPayload
            || !loadBPlan.LegacyAlgorithmRulesObservedPayloadFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "A", "B", "GB", "Ps", "R", "ResultDataList", "θb", "θs", "θw" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || !string.Equals(loadBPlan.LegacyAlgorithmRulesObservedPayloadSummary, "legacy algorithm rule observed payload fields observed 9/9; observed: A, B, GB, Ps, R, ResultDataList, θb, θs, θw", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(loadBPlan.RequiredPayloadFieldCoverageSummary)
            || string.IsNullOrWhiteSpace(loadBPlan.RequiredRatedParamFieldCoverageSummary)
            || string.IsNullOrWhiteSpace(loadBPlan.LegacyAlgorithmInputReadinessSummary)
            || string.IsNullOrWhiteSpace(loadBPlan.FormulaSignalSummary)
            || string.IsNullOrWhiteSpace(loadBPlan.LegacyAlgorithmRuleSummary)
            || !loadBPlan.FormulaSignalSummary.Contains("Tx²-Pl", StringComparison.Ordinal)
            || !loadBPlan.LegacyAlgorithmRuleSummary.Contains("cuC", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: stp.db adaptation plan dependency projection mismatch for LoadB.");
        }
    }
}
