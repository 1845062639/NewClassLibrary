using Microsoft.Data.Sqlite;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodDecisionSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

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
            if (!plan.SuggestedDecisionAnchorNextSteps.SequenceEqual(new[]
                {
                    "继续补齐热试验 firstSecondsInterval 判定依据：firstSecondsInterval",
                    "继续补齐热试验 HotStateType 分支字段：HotStateType",
                    "先补热试验 GB 温升分支关键字段：Rn"
                }, StringComparer.Ordinal)
                || !string.Equals(plan.SuggestedDecisionAnchorNextStepSummary, "继续补齐热试验 firstSecondsInterval 判定依据：firstSecondsInterval; 继续补齐热试验 HotStateType 分支字段：HotStateType; 先补热试验 GB 温升分支关键字段：Rn", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorGapPreviewSummary, "decision anchor gaps: gb-temperature-branch[partial]:Rn; first-seconds-interval[partial]:firstSecondsInterval; hot-state-branch[partial]:HotStateType", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: continue filling HeatRun firstSecondsInterval fields firstSecondsInterval; continue filling HeatRun HotStateType fields HotStateType; need HeatRun GB temperature branch fields Rn", StringComparison.Ordinal)
                || plan.LegacyDecisionAnchorResolutions.Count != 3
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "first-seconds-interval", StringComparison.Ordinal)
                    && resolution.PartiallyResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "Pn", "firstSecondsInterval" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.SequenceEqual(new[] { "Pn" }, StringComparer.Ordinal)
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "firstSecondsInterval" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "decision-interval", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "热试验 firstSecondsInterval 判定依据", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "firstSecondsInterval" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "继续补齐热试验 firstSecondsInterval 判定依据：firstSecondsInterval", StringComparison.Ordinal))
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "hot-state-branch", StringComparison.Ordinal)
                    && resolution.PartiallyResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "HotStateType", "θw" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.SequenceEqual(new[] { "θw" }, StringComparer.Ordinal)
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "HotStateType" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "热试验 HotStateType 分支字段", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "HotStateType" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "继续补齐热试验 HotStateType 分支字段：HotStateType", StringComparison.Ordinal))
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "gb-temperature-branch", StringComparison.Ordinal)
                    && resolution.PartiallyResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "GB", "Rn", "θb", "θs", "θw" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.SequenceEqual(new[] { "GB", "θb", "θs", "θw" }, StringComparer.Ordinal)
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "Rn" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "热试验 GB 温升分支关键字段", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Rn" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "先补热试验 GB 温升分支关键字段：Rn", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method decision smoke test failed: HeatRun decision-anchor projection mismatch. next='{plan.LegacyDecisionAnchorNextActionSummary}', suggested='{plan.SuggestedDecisionAnchorNextStepSummary}'");
            }

            return;
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal))
        {
            if (!plan.SuggestedDecisionAnchorNextSteps.SequenceEqual(new[]
                {
                    "继续补齐A法上游空载/热试验承接字段：θa",
                    "先补A法额定负载点回归结果：ResultDataList",
                    "先补A法 payload 额定量结果字段：Pcu1, Pcu2, η"
                }, StringComparer.Ordinal)
                || !string.Equals(plan.SuggestedDecisionAnchorNextStepSummary, "继续补齐A法上游空载/热试验承接字段：θa; 先补A法额定负载点回归结果：ResultDataList; 先补A法 payload 额定量结果字段：Pcu1, Pcu2, η", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorGapPreviewSummary, "decision anchor gaps: payload-rated-quantity-ready[missing]:Pcu1, Pcu2, η; upstream-ready[partial]:θa; rated-load-fit-grid[missing]:ResultDataList", StringComparison.Ordinal)
                || !string.Equals(plan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: continue filling LoadA upstream fields θa; need LoadA rated-load fit fields ResultDataList; need LoadA payload rated-result fields Pcu1, Pcu2, η", StringComparison.Ordinal)
                || plan.LegacyDecisionAnchorResolutions.Count != 3
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "upstream-ready", StringComparison.Ordinal)
                    && resolution.PartiallyResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "CoefficientOfPfe", "Pfw", "θa" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.SequenceEqual(new[] { "CoefficientOfPfe", "Pfw" }, StringComparer.Ordinal)
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "θa" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "upstream-carryover", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "A法上游空载/热试验承接字段", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "θa" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "继续补齐A法上游空载/热试验承接字段：θa", StringComparison.Ordinal))
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "rated-load-fit-grid", StringComparison.Ordinal)
                    && !resolution.PartiallyResolvedByObservedPayload
                    && !resolution.ResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.Count == 0
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "fit-grid", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "A法额定负载点回归结果", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "先补A法额定负载点回归结果：ResultDataList", StringComparison.Ordinal))
                || !plan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "payload-rated-quantity-ready", StringComparison.Ordinal)
                    && !resolution.PartiallyResolvedByObservedPayload
                    && !resolution.ResolvedByObservedPayload
                    && resolution.RequiredPayloadFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)
                    && resolution.ObservedPayloadFields.Count == 0
                    && resolution.MissingPayloadFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepCategory, "rated-quantity", StringComparison.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepFocus, "A法 payload 额定量结果字段", StringComparison.Ordinal)
                    && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)
                    && string.Equals(resolution.SuggestedNextStepSummary, "先补A法 payload 额定量结果字段：Pcu1, Pcu2, η", StringComparison.Ordinal)))
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
        command.CommandText = @"
SELECT Code, Method, COUNT(*)
FROM TestRecordItems
WHERE Code IN (
    '直流电阻测定',
    '空载试验',
    '空载特性试验',
    '热试验',
    'A法负载试验',
    'B法负载试验',
    '堵转试验',
    '堵转特性试验')
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
