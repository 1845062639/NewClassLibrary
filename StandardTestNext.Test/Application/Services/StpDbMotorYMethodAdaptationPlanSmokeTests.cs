using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodAdaptationPlanSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method adaptation plan smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYMethodAdaptationPlans();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no rows returned.");
        }

        var legacyCodeDistributionRows = service.ListMotorYLegacyCodeDistribution();
        if (legacyCodeDistributionRows.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no legacy-code distribution rows returned.");
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
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing row for {row.CanonicalCode}.");
            }

            if (snapshot.TotalCount != row.TotalCount
                || snapshot.BaselineCount != row.BaselineCount
                || snapshot.DominantCount != row.DominantCount
                || snapshot.SelectedCount != row.SelectedCount)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: numeric mismatch for {row.CanonicalCode}.");
            }

            AssertRoute(snapshot.BaselineRoute, row.CanonicalCode, row.BaselineMethod, $"baseline/{row.CanonicalCode}");
            AssertRoute(snapshot.DominantRoute, row.CanonicalCode, row.DominantMethod, $"dominant/{row.CanonicalCode}");
            AssertRoute(snapshot.SelectedRoute, row.CanonicalCode, row.SelectedMethod, $"selected/{row.CanonicalCode}");

            if (!string.Equals(snapshot.SelectionStrategy, row.SelectionStrategy, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selection strategy mismatch for {row.CanonicalCode}. expected={row.SelectionStrategy}, actual={snapshot.SelectionStrategy}");
            }

            if (snapshot.ShouldUseDominantRoute != row.ShouldUseDominantRoute)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant-route flag mismatch for {row.CanonicalCode}.");
            }

            if (Math.Abs(snapshot.BaselineShare - row.BaselineShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline share mismatch for {row.CanonicalCode}. expected={row.BaselineShare}, actual={snapshot.BaselineShare}");
            }

            if (Math.Abs(snapshot.DominantShare - row.DominantShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant share mismatch for {row.CanonicalCode}. expected={row.DominantShare}, actual={snapshot.DominantShare}");
            }

            if (Math.Abs(snapshot.SelectedShare - row.SelectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected share mismatch for {row.CanonicalCode}. expected={row.SelectedShare}, actual={snapshot.SelectedShare}");
            }

            if (Math.Abs(snapshot.DominantOverrideThreshold - DominantOverrideThreshold) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: override threshold mismatch for {row.CanonicalCode}. expected={DominantOverrideThreshold}, actual={snapshot.DominantOverrideThreshold}");
            }

            var selectedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.SelectedMethod)
                ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route missing for {row.CanonicalCode}:{row.SelectedMethod}.");
            if (!string.Equals(snapshot.AlgorithmEntry, selectedRoute.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.SettingsMethodName, selectedRoute.LegacySettingsMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.LegacyMethodName, selectedRoute.LegacyMethodName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route metadata mismatch for {row.CanonicalCode}.");
            }

            if (snapshot.DominantLeadCount != row.DominantLeadCount
                || snapshot.DominantLeadPercentagePoints != row.DominantLeadPercentagePoints)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: lead summary mismatch for {row.CanonicalCode}. expectedCount={row.DominantLeadCount}, actualCount={snapshot.DominantLeadCount}, expectedPp={row.DominantLeadPercentagePoints}, actualPp={snapshot.DominantLeadPercentagePoints}");
            }

            if (Math.Abs(snapshot.SelectedLeadCountVsBaseline - row.SelectedLeadCountVsBaseline) > 0.0001d
                || snapshot.SelectedLeadPercentagePointsVsBaseline != row.SelectedLeadPercentagePointsVsBaseline)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected-vs-baseline lead mismatch for {row.CanonicalCode}. expectedCount={row.SelectedLeadCountVsBaseline}, actualCount={snapshot.SelectedLeadCountVsBaseline}, expectedPp={row.SelectedLeadPercentagePointsVsBaseline}, actualPp={snapshot.SelectedLeadPercentagePointsVsBaseline}");
            }

            if (!string.Equals(snapshot.SelectionReason, row.SelectionReason, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selection reason mismatch for {row.CanonicalCode}. expected='{row.SelectionReason}', actual='{snapshot.SelectionReason}'");
            }

            if (!string.Equals(snapshot.SelectedMethodSummary, row.SelectedMethodSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected method summary mismatch for {row.CanonicalCode}. expected='{row.SelectedMethodSummary}', actual='{snapshot.SelectedMethodSummary}'");
            }

            if (!string.Equals(snapshot.BaselineDominantComparisonSummary, row.BaselineDominantComparisonSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline/dominant comparison summary mismatch for {row.CanonicalCode}. expected='{row.BaselineDominantComparisonSummary}', actual='{snapshot.BaselineDominantComparisonSummary}'");
            }

            if (snapshot.Distributions.Count != row.Distributions.Count)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution count mismatch for {row.CanonicalCode}.");
            }

            AssertLegacyCodeSelection(snapshot, connection, row.CanonicalCode);
            AssertLegacyCodeDistributionConsistency(snapshot, legacyCodeDistributionRows, row.CanonicalCode);
            AssertSampleCountReadiness(snapshot);

            foreach (var distribution in row.Distributions)
            {
                var actualDistribution = snapshot.Distributions.FirstOrDefault(x => x.MethodValue == distribution.MethodValue);
                if (actualDistribution is null)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing distribution {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (actualDistribution.Count != distribution.Count)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution count mismatch for {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (Math.Abs(actualDistribution.Share - distribution.Share) > 0.0001d)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution share mismatch for {row.CanonicalCode}:{distribution.MethodValue}. expected={distribution.Share}, actual={actualDistribution.Share}");
                }

                AssertRoute(actualDistribution.Route, row.CanonicalCode, distribution.MethodValue, $"distribution/{row.CanonicalCode}:{distribution.MethodValue}");
            }
        }
    }

    private static void AssertLegacyCodeSelection(
        MotorYMethodAdaptationPlanSnapshot snapshot,
        SqliteConnection connection,
        string canonicalCode)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COALESCE(Code, ''), COUNT(*)
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
GROUP BY COALESCE(Code, '');";

        var rows = new List<(string LegacyCode, int Count)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            if (!string.Equals(MotorYLegacyItemCodeNormalizer.Normalize(legacyCode), canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            rows.Add((legacyCode, reader.GetInt32(1)));
        }

        var total = rows.Sum(x => x.Count);
        var ordered = rows
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();
        var expected = ordered.FirstOrDefault();
        var expectedShare = total <= 0
            ? 0d
            : Math.Round((double)expected.Count / total, 4, MidpointRounding.AwayFromZero);
        var expectedSummary = expected == default
            ? $"legacy code selection unavailable for {canonicalCode}"
            : $"recommended legacy code '{expected.LegacyCode}' for {canonicalCode} ({expected.Count}/{total}, {(int)Math.Round(expectedShare * 100d, MidpointRounding.AwayFromZero)}pp)";

        if (!string.Equals(snapshot.RecommendedLegacyCode, expected.LegacyCode, StringComparison.Ordinal)
            || !string.Equals(snapshot.DominantLegacyCode, expected.LegacyCode, StringComparison.Ordinal)
            || snapshot.RecommendedLegacyCodeCount != expected.Count
            || Math.Abs(snapshot.RecommendedLegacyCodeShare - expectedShare) > 0.0001d
            || !string.Equals(snapshot.LegacyCodeSelectionSummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code selection mismatch for {canonicalCode}. expectedCode={expected.LegacyCode}, actualCode={snapshot.RecommendedLegacyCode}, expectedCount={expected.Count}, actualCount={snapshot.RecommendedLegacyCodeCount}, expectedShare={expectedShare}, actualShare={snapshot.RecommendedLegacyCodeShare}, expectedSummary='{expectedSummary}', actualSummary='{snapshot.LegacyCodeSelectionSummary}'");
        }

        if (snapshot.LegacyCodeDistributions.Count != ordered.Length)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution count mismatch for {canonicalCode}. expected={ordered.Length}, actual={snapshot.LegacyCodeDistributions.Count}");
        }

        foreach (var row in ordered)
        {
            var actual = snapshot.LegacyCodeDistributions.FirstOrDefault(x => string.Equals(x.LegacyCode, row.LegacyCode, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing legacy-code distribution {canonicalCode}/{row.LegacyCode}.");
            }

            var share = total <= 0 ? 0d : Math.Round((double)row.Count / total, 4, MidpointRounding.AwayFromZero);
            if (actual.Count != row.Count || Math.Abs(actual.Share - share) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution mismatch for {canonicalCode}/{row.LegacyCode}. expectedCount={row.Count}, actualCount={actual.Count}, expectedShare={share}, actualShare={actual.Share}");
            }
        }
    }

    private static void AssertLegacyCodeDistributionConsistency(
        MotorYMethodAdaptationPlanSnapshot snapshot,
        IReadOnlyList<StpDbMotorYLegacyCodeDistributionSnapshot> legacyCodeDistributionRows,
        string canonicalCode)
    {
        var expectedRows = legacyCodeDistributionRows
            .Where(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .GroupBy(x => x.LegacyCode, StringComparer.Ordinal)
            .Select(group => new
            {
                LegacyCode = group.Key,
                Count = group.Sum(x => x.Count)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();

        if (expectedRows.Length != snapshot.LegacyCodeDistributions.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check count mismatch for {canonicalCode}. expected={expectedRows.Length}, actual={snapshot.LegacyCodeDistributions.Count}");
        }

        var total = expectedRows.Sum(x => x.Count);
        foreach (var row in expectedRows)
        {
            var expectedShare = total <= 0
                ? 0d
                : Math.Round((double)row.Count / total, 4, MidpointRounding.AwayFromZero);
            var actual = snapshot.LegacyCodeDistributions.FirstOrDefault(x => string.Equals(x.LegacyCode, row.LegacyCode, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check missing row {canonicalCode}/{row.LegacyCode}.");
            }

            if (actual.Count != row.Count || Math.Abs(actual.Share - expectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check mismatch for {canonicalCode}/{row.LegacyCode}. expectedCount={row.Count}, actualCount={actual.Count}, expectedShare={expectedShare}, actualShare={actual.Share}");
            }
        }

        var expectedRecommended = expectedRows.FirstOrDefault();
        if (expectedRecommended is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: no expected recommended legacy code for {canonicalCode}.");
        }

        if (!string.Equals(snapshot.RecommendedLegacyCode, expectedRecommended.LegacyCode, StringComparison.Ordinal)
            || !string.Equals(snapshot.DominantLegacyCode, expectedRecommended.LegacyCode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check recommended/dominant mismatch for {canonicalCode}. expected='{expectedRecommended.LegacyCode}', recommended='{snapshot.RecommendedLegacyCode}', dominant='{snapshot.DominantLegacyCode}'");
        }
    }

    private static void AssertSampleCountReadiness(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        var expectedRawReady = snapshot.RawDataSampleCount >= snapshot.MinimumRawSampleCount;
        var expectedStructuredPayloadReady = snapshot.StructuredPayloadSampleCount >= snapshot.MinimumStructuredPayloadSampleCount;
        var expectedStructuredResultReady = snapshot.StructuredResultSampleCount >= snapshot.MinimumStructuredResultSampleCount;

        var expectedRawSummary = snapshot.MinimumRawSampleCount <= 0
            ? $"raw sample count requirement not set; observed {snapshot.RawDataSampleCount}"
            : expectedRawReady
                ? $"raw sample count ready {snapshot.RawDataSampleCount}/{snapshot.MinimumRawSampleCount}"
                : $"raw sample count insufficient {snapshot.RawDataSampleCount}/{snapshot.MinimumRawSampleCount}";
        var expectedStructuredPayloadSummary = snapshot.MinimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count requirement not set; observed {snapshot.StructuredPayloadSampleCount}"
            : expectedStructuredPayloadReady
                ? $"structured payload sample count ready {snapshot.StructuredPayloadSampleCount}/{snapshot.MinimumStructuredPayloadSampleCount}"
                : $"structured payload sample count insufficient {snapshot.StructuredPayloadSampleCount}/{snapshot.MinimumStructuredPayloadSampleCount}";
        var expectedStructuredResultSummary = snapshot.MinimumStructuredResultSampleCount <= 0
            ? $"structured result sample count requirement not set; observed {snapshot.StructuredResultSampleCount}"
            : expectedStructuredResultReady
                ? $"structured result sample count ready {snapshot.StructuredResultSampleCount}/{snapshot.MinimumStructuredResultSampleCount}"
                : $"structured result sample count insufficient {snapshot.StructuredResultSampleCount}/{snapshot.MinimumStructuredResultSampleCount}";

        var expectedRawGap = Math.Max(0, snapshot.MinimumRawSampleCount - snapshot.RawDataSampleCount);
        var expectedStructuredPayloadGap = Math.Max(0, snapshot.MinimumStructuredPayloadSampleCount - snapshot.StructuredPayloadSampleCount);
        var expectedStructuredResultGap = Math.Max(0, snapshot.MinimumStructuredResultSampleCount - snapshot.StructuredResultSampleCount);
        var expectedRawDecisionSummary = snapshot.MinimumRawSampleCount <= 0
            ? $"raw sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.RawDataSampleCount}"
            : expectedRawReady
                ? $"raw sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.RawDataSampleCount} >= required {snapshot.MinimumRawSampleCount}"
                : $"raw sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.RawDataSampleCount}, still need {expectedRawGap} more samples to reach {snapshot.MinimumRawSampleCount}";
        var expectedStructuredPayloadDecisionSummary = snapshot.MinimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.StructuredPayloadSampleCount}"
            : expectedStructuredPayloadReady
                ? $"structured payload sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.StructuredPayloadSampleCount} >= required {snapshot.MinimumStructuredPayloadSampleCount}"
                : $"structured payload sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.StructuredPayloadSampleCount}, still need {expectedStructuredPayloadGap} more samples to reach {snapshot.MinimumStructuredPayloadSampleCount}";
        var expectedStructuredResultDecisionSummary = snapshot.MinimumStructuredResultSampleCount <= 0
            ? $"structured result sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.StructuredResultSampleCount}"
            : expectedStructuredResultReady
                ? $"structured result sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.StructuredResultSampleCount} >= required {snapshot.MinimumStructuredResultSampleCount}"
                : $"structured result sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.StructuredResultSampleCount}, still need {expectedStructuredResultGap} more samples to reach {snapshot.MinimumStructuredResultSampleCount}";

        if (snapshot.RawSampleCountReady != expectedRawReady
            || snapshot.StructuredPayloadSampleCountReady != expectedStructuredPayloadReady
            || snapshot.StructuredResultSampleCountReady != expectedStructuredResultReady
            || snapshot.RawSampleCountGap != expectedRawGap
            || snapshot.StructuredPayloadSampleCountGap != expectedStructuredPayloadGap
            || snapshot.StructuredResultSampleCountGap != expectedStructuredResultGap
            || !string.Equals(snapshot.RawSampleCountReadinessSummary, expectedRawSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredPayloadSampleCountReadinessSummary, expectedStructuredPayloadSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredResultSampleCountReadinessSummary, expectedStructuredResultSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.RawSampleCountDecisionSummary, expectedRawDecisionSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredPayloadSampleCountDecisionSummary, expectedStructuredPayloadDecisionSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredResultSampleCountDecisionSummary, expectedStructuredResultDecisionSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: sample-count readiness mismatch for {snapshot.CanonicalCode}. raw='{snapshot.RawSampleCountReadinessSummary}' gap={snapshot.RawSampleCountGap} decision='{snapshot.RawSampleCountDecisionSummary}', structuredPayload='{snapshot.StructuredPayloadSampleCountReadinessSummary}' gap={snapshot.StructuredPayloadSampleCountGap} decision='{snapshot.StructuredPayloadSampleCountDecisionSummary}', structuredResult='{snapshot.StructuredResultSampleCountReadinessSummary}' gap={snapshot.StructuredResultSampleCountGap} decision='{snapshot.StructuredResultSampleCountDecisionSummary}'");
        }
    }

    private static void AssertRoute(MotorYLegacyAlgorithmRoute? route, string canonicalCode, int methodValue, string context)
    {
        var expected = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, methodValue)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: expected route missing for {context}.");

        if (route is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: route missing for {context}.");
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
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: route projection mismatch for {context}.");
        }
    }

    private static (string CanonicalCode, int TotalCount, int BaselineMethod, int BaselineCount, double BaselineShare, int DominantMethod, int DominantCount, double DominantShare, int SelectedMethod, int SelectedCount, double SelectedShare, string SelectionStrategy, bool ShouldUseDominantRoute, int DominantLeadCount, int DominantLeadPercentagePoints, double SelectedLeadCountVsBaseline, int SelectedLeadPercentagePointsVsBaseline, string SelectionReason, string SelectedMethodSummary, string BaselineDominantComparisonSummary, IReadOnlyList<(int MethodValue, int Count, double Share)> Distributions) BuildExpected(
        SqliteConnection connection,
        string canonicalCode,
        int baselineMethod)
    {
        var rows = LoadCounts(connection, canonicalCode);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: no source rows for {canonicalCode}.");
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
        var shouldUseDominantRoute = baselineMethod != dominant.Key
            && dominantShare >= DominantOverrideThreshold;
        var selectedMethod = shouldUseDominantRoute ? dominant.Key : baselineMethod;
        var selectedCount = shouldUseDominantRoute ? dominant.Value : baselineCount;
        var selectionStrategy = shouldUseDominantRoute
            ? "dominant-threshold-over-baseline"
            : "baseline";
        var baselineShare = totalCount <= 0
            ? 0d
            : Math.Round((double)baselineCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var selectedShare = totalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var dominantLeadCount = Math.Max(0, dominant.Value - baselineCount);
        var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((dominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
        var selectedLeadCountVsBaseline = Math.Max(0d, selectedCount - baselineCount);
        var selectedLeadPercentagePointsVsBaseline = Math.Max(0, (int)Math.Round((selectedShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
        var selectionReason = shouldUseDominantRoute
            ? $"selected dominant method {dominant.Key} over baseline {baselineMethod} because dominant share {dominantShare:P2} reached threshold {DominantOverrideThreshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)"
            : baselineMethod == dominant.Key
                ? $"kept baseline method {selectedMethod} because baseline already matches dominant distribution ({dominantShare:P2})"
                : $"kept baseline method {baselineMethod} because dominant method {dominant.Key} share {dominantShare:P2} did not reach threshold {DominantOverrideThreshold:P0}";
        var selectedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, selectedMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route missing for {canonicalCode}:{selectedMethod}.");
        var selectedMethodSummary = $"selected {selectedRoute.LegacyMethodName ?? canonicalCode} method {selectedMethod} ({selectedRoute.VariantKind}) covering {selectedCount}/{totalCount} items ({selectedShare:P2})";
        var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, baselineMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline route missing for {canonicalCode}:{baselineMethod}.");
        var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, dominant.Key)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant route missing for {canonicalCode}:{dominant.Key}.");
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

        return (canonicalCode, totalCount, baselineMethod, baselineCount, baselineShare, dominant.Key, dominant.Value, dominantShare, selectedMethod, selectedCount, selectedShare, selectionStrategy, shouldUseDominantRoute, dominantLeadCount, dominantLeadPercentagePoints, selectedLeadCountVsBaseline, selectedLeadPercentagePointsVsBaseline, selectionReason, selectedMethodSummary, baselineDominantComparisonSummary, distributions);
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
