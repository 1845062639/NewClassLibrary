using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyAlgorithmDependencyCatalogSmokeTests
{
    public static void Run()
    {
        var expected = new[]
        {
            (MotorYTestMethodCodes.DcResistance, false, Array.Empty<string>(), new[] { "Ruv", "Rvw", "Rwu", "R1", "θ1c" }, Array.Empty<string>()),
            (MotorYTestMethodCodes.NoLoad, false, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" }, Array.Empty<string>()),
            (MotorYTestMethodCodes.HeatRun, true, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" }, new[] { "GB" }),
            (MotorYTestMethodCodes.LoadA, false, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" }, Array.Empty<string>()),
            (MotorYTestMethodCodes.LoadB, true, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" }, new[] { "GB" }),
            (MotorYTestMethodCodes.LockedRotor, false, new[] { MotorYTestMethodCodes.NoLoad }, new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" }, Array.Empty<string>())
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
            || !contract.MissingUpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, StringComparer.Ordinal)
            || contract.UpstreamDependenciesSatisfied
            || !string.Equals(contract.UpstreamDependencySummary, "upstream dependencies missing 2/2: NoLoad, HeatRun", StringComparison.Ordinal)
            || !contract.RequiredPayloadFields.Contains("θw", StringComparer.Ordinal)
            || !contract.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
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
            || contract.LegacyAlgorithmRules.Count == 0
            || string.IsNullOrWhiteSpace(contract.FormulaSignalSummary)
            || string.IsNullOrWhiteSpace(contract.LegacyAlgorithmRuleSummary)
            || !string.Equals(contract.RequiredRatedParamFieldCoverageSummary, "rated param required fields covered 0/1 (0pp); missing: GB", StringComparison.Ordinal)
            || !string.Equals(contract.LegacyAlgorithmInputReadinessSummary, "legacy algorithm inputs incomplete; upstream dependencies missing 2/2: NoLoad, HeatRun; payload required fields covered 0/13 (0pp); missing: RawDataList, CoefficientOfPfe, Pfw, R1c, θ1c, θw, θb, PolePairs, Pn, Un, ΔT, K1, K2; rated param required fields covered 0/1 (0pp); missing: GB", StringComparison.Ordinal)
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
            || loadBPlan.MissingUpstreamCanonicalCodes.Count != 0
            || !loadBPlan.UpstreamDependenciesSatisfied
            || !string.Equals(loadBPlan.UpstreamDependencySummary, "upstream dependencies satisfied (NoLoad + HeatRun)", StringComparison.Ordinal)
            || !loadBPlan.RequiredPayloadFields.Contains("θw", StringComparer.Ordinal)
            || !loadBPlan.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal)
            || !loadBPlan.SamplePayloadAvailable
            || loadBPlan.CoveredRequiredPayloadFields.Count == 0
            || loadBPlan.RequiredPayloadFieldCoverageRatio <= 0d
            || loadBPlan.RequiredPayloadFieldCoveragePercentagePoints <= 0
            || !loadBPlan.RatedParamsAvailable
            || loadBPlan.CoveredRequiredRatedParamFields.Count == 0
            || loadBPlan.RequiredRatedParamFieldCoverageRatio <= 0d
            || loadBPlan.RequiredRatedParamFieldCoveragePercentagePoints <= 0
            || !loadBPlan.LegacyAlgorithmInputsReady
            || loadBPlan.FormulaSignals.Count == 0
            || loadBPlan.LegacyAlgorithmRules.Count == 0
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
