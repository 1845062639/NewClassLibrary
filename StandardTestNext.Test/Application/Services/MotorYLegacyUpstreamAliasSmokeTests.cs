using Microsoft.Data.Sqlite;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyUpstreamAliasSmokeTests
{
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        ShouldExposeLegacyAliasesInDependencyCatalog();
        ShouldExposeObservedLegacyUpstreamCodeDistributionFromStpDb();
    }

    private static void ShouldExposeLegacyAliasesInDependencyCatalog()
    {
        var loadB = MotorYLegacyAlgorithmDependencyCatalog.TryGet(MotorYTestMethodCodes.LoadB)
            ?? throw new InvalidOperationException("Motor_Y legacy upstream alias smoke test failed: Load_B profile missing.");

        if (!loadB.UpstreamLegacyAliases.TryGetValue(MotorYTestMethodCodes.NoLoad, out var noLoadAliases)
            || !noLoadAliases.SequenceEqual(new[] { "空载特性完全试验", "空载特性测量", "空载特性试验", "空载试验", "空载试验（出厂）" }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: Load_B NoLoad aliases mismatch. actual=[{string.Join(", ", noLoadAliases ?? Array.Empty<string>())}]");
        }

        if (!loadB.UpstreamLegacyAliases.TryGetValue(MotorYTestMethodCodes.HeatRun, out var heatAliases)
            || !heatAliases.SequenceEqual(new[] { "温度计法热试验", "热试验", "热试验2", "陪试热试验" }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: Load_B HeatRun aliases mismatch. actual=[{string.Join(", ", heatAliases ?? Array.Empty<string>())}]");
        }
    }

    private static void ShouldExposeObservedLegacyUpstreamCodeDistributionFromStpDb()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for Motor_Y legacy upstream alias smoke test: {DbPath}");
        }

        var snapshotService = new StpDbSnapshotQueryService(DbPath);
        var plans = snapshotService.ListMotorYMethodAdaptationPlans();
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Motor_Y legacy upstream alias smoke test failed: Load_B adaptation plan missing.");

        AssertUpstreamDistribution(loadBPlan, MotorYTestMethodCodes.NoLoad, "空载特性试验", 233, 0.66d, 5);
        AssertUpstreamDistribution(loadBPlan, MotorYTestMethodCodes.HeatRun, "热试验", 265, 1d, 4);

        if (!loadBPlan.ObservedUpstreamLegacyCodes.TryGetValue(MotorYTestMethodCodes.NoLoad, out var observedNoLoadLegacyCodes)
            || !observedNoLoadLegacyCodes.SequenceEqual(new[] { "空载特性完全试验", "空载特性测量", "空载特性试验", "空载试验", "空载试验（出厂）" }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: observed NoLoad legacy aliases mismatch. actual=[{string.Join(", ", observedNoLoadLegacyCodes ?? Array.Empty<string>())}]");
        }

        if (!loadBPlan.ObservedUpstreamLegacyCodes.TryGetValue(MotorYTestMethodCodes.HeatRun, out var observedHeatLegacyCodes)
            || !observedHeatLegacyCodes.SequenceEqual(new[] { "陪试热试验", "温度计法热试验", "热试验", "热试验2" }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: observed HeatRun legacy aliases mismatch. actual=[{string.Join(", ", observedHeatLegacyCodes ?? Array.Empty<string>())}]");
        }

        var contract = MotorYMethodAdaptationPlanContractMapper.Map(
            new MotorYMethodDecisionSnapshot
            {
                CanonicalCode = MotorYTestMethodCodes.LoadB,
                TotalCount = 1,
                BaselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
                BaselineCount = 1,
                DominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
                DominantCount = 1,
                RecommendedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
                RecommendedStrategy = "baseline",
                ShouldPrioritizeDominantOverBaseline = false,
                DominantShare = 1d,
                BaselineShare = 1d,
                DominantOverrideThreshold = 0.7d,
                DominantLeadCount = 0,
                DominantLeadPercentagePoints = 0,
                RecommendationReason = "baseline method remains selected for LoadB because dominant route does not exceed override threshold 70pp",
                RecommendedMethodSummary = "recommended method 5 for LoadB via baseline strategy (1/1, 100pp)",
                BaselineDominantComparisonSummary = "baseline method 5 vs dominant method 5 => same route (100pp vs 100pp)",
                Distributions = new[]
                {
                    new MotorYMethodDistributionSnapshot
                    {
                        MethodValue = 5,
                        Count = 1,
                        Share = 1d,
                        Route = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5)
                    }
                }
            },
            route => route is null
                ? null
                : new MotorYBuildProfileContract
                {
                    CanonicalCode = route.CanonicalCode,
                    MethodValue = route.MethodValue,
                    MethodKey = route.MethodKey,
                    ProfileKey = route.ProfileKey,
                    LegacyEnumName = route.LegacyEnumName,
                    LegacyFormName = route.LegacyFormName,
                    LegacyAlgorithmEntry = route.LegacyAlgorithmEntry,
                    LegacyMethodName = route.LegacyMethodName,
                    LegacySettingsMethodName = route.LegacySettingsMethodName,
                    VariantKind = route.VariantKind,
                    AlgorithmFamily = route.AlgorithmFamily,
                    IsBaselineMethod = route.IsBaselineMethod
                });

        if (contract.UpstreamLegacyCodeDistributions.Count != 9)
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: contract upstream legacy-code distribution count mismatch. expected=9, actual={contract.UpstreamLegacyCodeDistributions.Count}");
        }

        if (!contract.UpstreamLegacyCodeDistributions.All(x => x.Count == 0 && Math.Abs(x.Share) < 0.0001d))
        {
            throw new InvalidOperationException("Motor_Y legacy upstream alias smoke test failed: builder/query-side empty upstream distributions should default to zero counts.");
        }
    }

    private static void AssertUpstreamDistribution(
        MotorYMethodAdaptationPlanSnapshot plan,
        string canonicalCode,
        string expectedTopLegacyCode,
        int expectedTopCount,
        double expectedTopShare,
        int expectedAliasCount)
    {
        if (!plan.UpstreamLegacyAliases.TryGetValue(canonicalCode, out var aliases) || aliases.Count != expectedAliasCount)
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: alias count mismatch for {plan.CanonicalCode}/{canonicalCode}. expected={expectedAliasCount}, actual={(aliases?.Count ?? 0)}");
        }

        var distributions = plan.UpstreamLegacyCodeDistributions
            .Where(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .ToArray();
        if (distributions.Length != expectedAliasCount)
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: upstream distribution count mismatch for {plan.CanonicalCode}/{canonicalCode}. expected={expectedAliasCount}, actual={distributions.Length}");
        }

        var top = distributions.First();
        if (!string.Equals(top.LegacyCode, expectedTopLegacyCode, StringComparison.Ordinal)
            || top.Count != expectedTopCount
            || Math.Abs(top.Share - expectedTopShare) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y legacy upstream alias smoke test failed: top upstream distribution mismatch for {plan.CanonicalCode}/{canonicalCode}. expected={expectedTopLegacyCode}/{expectedTopCount}/{expectedTopShare}, actual={top.LegacyCode}/{top.Count}/{top.Share}");
        }
    }
}
