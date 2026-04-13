using Microsoft.Data.Sqlite;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodDecisionSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));
    private static readonly string[] MotorYLegacyDecisionSmokeAliases =
    [
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.DcResistance),
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.NoLoad),
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.HeatRun),
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.LoadA),
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.LoadB),
        ..MotorYLegacyItemCodeNormalizer.GetLegacyAliases(MotorYTestMethodCodes.LockedRotor)
    ];


    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method decision smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYMethodDecisions();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method decision smoke test failed: no rows returned.");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expected = new[]
        {
            BuildExpected(connection, MotorYTestMethodCodes.DcResistance, 1),
            BuildExpected(connection, MotorYTestMethodCodes.NoLoad, 0),
            BuildExpected(connection, MotorYTestMethodCodes.HeatRun, 3),
            BuildExpected(connection, MotorYTestMethodCodes.LoadA, 4),
            BuildExpected(connection, MotorYTestMethodCodes.LoadB, 5),
            BuildExpected(connection, MotorYTestMethodCodes.LockedRotor, 11)
        };

        foreach (var row in expected)
        {
            var snapshot = actual.FirstOrDefault(x => string.Equals(x.CanonicalCode, row.CanonicalCode, StringComparison.Ordinal));
            if (snapshot is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: missing row for {row.CanonicalCode}.");
            }

            if (snapshot.TotalCount != row.TotalCount
                || snapshot.BaselineCount != row.BaselineCount
                || snapshot.DominantCount != row.DominantCount)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: numeric mismatch for {row.CanonicalCode}.");
            }

            AssertRoute(snapshot.BaselineRoute, row.CanonicalCode, row.BaselineMethod, $"baseline/{row.CanonicalCode}");
            AssertRoute(snapshot.DominantRoute, row.CanonicalCode, row.DominantMethod, $"dominant/{row.CanonicalCode}");

            var expectedShare = row.TotalCount <= 0
                ? 0d
                : Math.Round((double)row.DominantCount / row.TotalCount, 4, MidpointRounding.AwayFromZero);
            var shouldPrioritizeDominant = row.BaselineMethod != row.DominantMethod
                && expectedShare >= DominantOverrideThreshold;
            var expectedRecommendedMethod = shouldPrioritizeDominant
                ? row.DominantMethod
                : row.BaselineMethod;
            var expectedRecommendedStrategy = shouldPrioritizeDominant
                ? "dominant-threshold-over-baseline"
                : "baseline";
            AssertRoute(snapshot.RecommendedRoute, row.CanonicalCode, expectedRecommendedMethod, $"recommended/{row.CanonicalCode}");
            if (!string.Equals(snapshot.RecommendedStrategy, expectedRecommendedStrategy, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: recommended strategy mismatch for {row.CanonicalCode}. expected={expectedRecommendedStrategy}, actual={snapshot.RecommendedStrategy}");
            }

            if (Math.Abs(snapshot.DominantShare - expectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: dominant share mismatch for {row.CanonicalCode}. expected={expectedShare}, actual={snapshot.DominantShare}");
            }

            if (Math.Abs(snapshot.DominantOverrideThreshold - DominantOverrideThreshold) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: override threshold mismatch for {row.CanonicalCode}. expected={DominantOverrideThreshold}, actual={snapshot.DominantOverrideThreshold}");
            }

            if (snapshot.ShouldPrioritizeDominantOverBaseline != shouldPrioritizeDominant)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: prioritize flag mismatch for {row.CanonicalCode}.");
            }

            if (!string.Equals(snapshot.RecommendedMethodSummary, row.RecommendedMethodSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: recommended summary mismatch for {row.CanonicalCode}. expected={row.RecommendedMethodSummary}, actual={snapshot.RecommendedMethodSummary}");
            }

            if (!string.Equals(snapshot.BaselineDominantComparisonSummary, row.BaselineDominantComparisonSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: baseline/dominant summary mismatch for {row.CanonicalCode}. expected={row.BaselineDominantComparisonSummary}, actual={snapshot.BaselineDominantComparisonSummary}");
            }

            if (snapshot.Distributions.Count != row.Distributions.Count)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: distribution count mismatch for {row.CanonicalCode}.");
            }

            foreach (var distribution in row.Distributions)
            {
                var actualDistribution = snapshot.Distributions.FirstOrDefault(x => x.MethodValue == distribution.MethodValue);
                if (actualDistribution is null)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: missing distribution {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (actualDistribution.Count != distribution.Count)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: distribution count mismatch for {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (Math.Abs(actualDistribution.Share - distribution.Share) > 0.0001d)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: distribution share mismatch for {row.CanonicalCode}:{distribution.MethodValue}. expected={distribution.Share}, actual={actualDistribution.Share}");
                }

                AssertRoute(actualDistribution.Route, row.CanonicalCode, distribution.MethodValue, $"distribution/{row.CanonicalCode}:{distribution.MethodValue}");
            }

            AssertDecisionAnchorProjection(snapshot, row.CanonicalCode);
        }
    }

    private static void AssertDecisionAnchorProjection(MotorYMethodDecisionSnapshot snapshot, string canonicalCode)
    {
        var service = new StpDbSnapshotQueryService(DbPath);
        var plan = service.ListMotorYMethodAdaptationPlans().FirstOrDefault(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: missing adaptation plan for {canonicalCode}.");

        if (!string.Equals(snapshot.RecommendedRoute?.CanonicalCode, plan.CanonicalCode, StringComparison.Ordinal)
            || snapshot.RecommendedRoute?.MethodValue != plan.SelectedRoute?.MethodValue)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: route mismatch between decision and adaptation plan for {canonicalCode}.");
        }

        if (!string.Equals(snapshot.RecommendedMethodSummary, plan.SelectedMethodSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.BaselineDominantComparisonSummary, plan.BaselineDominantComparisonSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: summary mismatch between decision and adaptation plan for {canonicalCode}.");
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal))
        {
            if (!string.Equals(plan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: need HeatRun firstSecondsInterval fields Pn; need HeatRun HotStateType fields HotStateType", StringComparison.Ordinal)
                || !string.Equals(plan.SuggestedDecisionAnchorNextStepSummary, "先补热试验 firstSecondsInterval 判定依据：Pn; 先补热试验 HotStateType 分支字段：HotStateType", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorGapPreviewSummary, "decision anchor gaps: first-seconds-interval[missing]:Pn; hot-state-branch[missing]:HotStateType", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: HeatRun decision-anchor projection mismatch. next='{plan.LegacyDecisionAnchorNextActionSummary}', suggested='{plan.SuggestedDecisionAnchorNextStepSummary}'");
            }

            return;
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal))
        {
            if (!string.Equals(plan.SuggestedDecisionAnchorNextStepSummary, "继续补齐A法上游空载/热试验承接字段：CoefficientOfPfe, Pfw", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: continue filling LoadA upstream fields CoefficientOfPfe, Pfw", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: LoadA decision-anchor projection mismatch. next='{plan.LegacyDecisionAnchorNextActionSummary}', suggested='{plan.SuggestedDecisionAnchorNextStepSummary}'");
            }
        }
    }

    private static void AssertRoute(MotorYLegacyAlgorithmRoute? route, string canonicalCode, int methodValue, string context)
    {
        var expected = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, methodValue)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: expected route missing for {context}.");

        if (route is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: route missing for {context}.");
        }

        if (!string.Equals(route.CanonicalCode, expected.CanonicalCode, StringComparison.Ordinal)
            || route.MethodValue != expected.MethodValue
            || !string.Equals(route.MethodKey, expected.MethodKey, StringComparison.Ordinal)
            || !string.Equals(route.ProfileKey, expected.ProfileKey, StringComparison.Ordinal)
            || !string.Equals(route.VariantKind, expected.VariantKind, StringComparison.Ordinal)
            || !string.Equals(route.AlgorithmFamily, expected.AlgorithmFamily, StringComparison.Ordinal)
            || !string.Equals(route.LegacyEnumName, expected.LegacyEnumName, StringComparison.Ordinal)
            || !string.Equals(route.LegacyFormName, expected.LegacyFormName, StringComparison.Ordinal)
            || !string.Equals(route.LegacyAlgorithmEntry, expected.LegacyAlgorithmEntry, StringComparison.Ordinal)
            || !string.Equals(route.LegacyMethodName, expected.LegacyMethodName, StringComparison.Ordinal)
            || !string.Equals(route.LegacySettingsMethodName, expected.LegacySettingsMethodName, StringComparison.Ordinal)
            || route.IsBaselineMethod != expected.IsBaselineMethod)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: route projection mismatch for {context}.");
        }
    }

    private static (string CanonicalCode, int TotalCount, int BaselineMethod, int BaselineCount, int DominantMethod, int DominantCount, int RecommendedMethod, string RecommendedStrategy, string RecommendedMethodSummary, string BaselineDominantComparisonSummary, IReadOnlyList<(int MethodValue, int Count, double Share)> Distributions) BuildExpected(
        SqliteConnection connection,
        string canonicalCode,
        int baselineMethod)
    {
        var rows = LoadCounts(connection, canonicalCode);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: no source rows for {canonicalCode}.");
        }

        var baselineCount = rows.TryGetValue(baselineMethod, out var baseline) ? baseline : 0;
        var dominant = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .First();
        var totalCount = rows.Values.Sum();
        var dominantShare = totalCount <= 0
            ? 0d
            : Math.Round((double)dominant.Value / totalCount, 4, MidpointRounding.AwayFromZero);
        var shouldPrioritizeDominant = baselineMethod != dominant.Key
            && dominantShare >= DominantOverrideThreshold;
        var recommendedMethod = shouldPrioritizeDominant ? dominant.Key : baselineMethod;
        var recommendedStrategy = shouldPrioritizeDominant
            ? "dominant-threshold-over-baseline"
            : "baseline";
        var selectedCount = shouldPrioritizeDominant ? dominant.Value : baselineCount;
        var selectedShare = totalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var baselineShare = totalCount <= 0
            ? 0d
            : Math.Round((double)baselineCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var recommendedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, recommendedMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: recommended route missing for {canonicalCode}:{recommendedMethod}.");
        var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, baselineMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: baseline route missing for {canonicalCode}:{baselineMethod}.");
        var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, dominant.Key)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: dominant route missing for {canonicalCode}:{dominant.Key}.");
        var recommendedMethodSummary = $"selected {recommendedRoute.LegacyMethodName} method {recommendedMethod} ({recommendedRoute.VariantKind}) covering {selectedCount}/{totalCount} items ({selectedShare:P2})";
        var baselineDominantComparisonSummary = $"baseline {baselineMethod} ({baselineRoute.VariantKind})={baselineCount}/{totalCount} ({baselineShare:P2}), dominant {dominant.Key} ({dominantRoute.VariantKind})={dominant.Value}/{totalCount} ({dominantShare:P2})";
        var distributions = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .Select(x => (
                MethodValue: x.Key,
                Count: x.Value,
                Share: totalCount <= 0
                    ? 0d
                    : Math.Round((double)x.Value / totalCount, 4, MidpointRounding.AwayFromZero)))
            .ToArray();

        return (canonicalCode, totalCount, baselineMethod, baselineCount, dominant.Key, dominant.Value, recommendedMethod, recommendedStrategy, recommendedMethodSummary, baselineDominantComparisonSummary, distributions);
    }

    private static Dictionary<int, int> LoadCounts(SqliteConnection connection, string canonicalCode)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT Code, Method, COUNT(*)
FROM TestRecordItems
WHERE Code IN (
    {string.Join(",\n    ", MotorYLegacyDecisionSmokeAliases.Select(code => $"'{code}'"))}
)
  AND Method IS NOT NULL
GROUP BY Code, Method;";

        var result = new Dictionary<int, int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var normalized = MotorYLegacyItemCodeNormalizer.Normalize(reader.GetString(0));
            if (!string.Equals(normalized, canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            var method = reader.GetInt32(1);
            var count = reader.GetInt32(2);
            result[method] = result.TryGetValue(method, out var current)
                ? current + count
                : count;
        }

        return result;
    }
}
