using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodRecommendationSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method recommendation smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYMethodRecommendations();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method recommendation smoke test failed: no rows returned.");
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
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: missing row for {row.CanonicalCode}.");
            }

            if (snapshot.TotalCount != row.TotalCount
                || snapshot.BaselineMethod != row.BaselineMethod
                || snapshot.BaselineCount != row.BaselineCount
                || snapshot.DominantMethod != row.DominantMethod
                || snapshot.DominantCount != row.DominantCount)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: numeric mismatch for {row.CanonicalCode}.");
            }

            AssertExplicitMethodDistributionBaseline(snapshot);

            if (!string.Equals(snapshot.BaselineMethodKey, $"{row.CanonicalCode}:{row.BaselineMethod}", StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantMethodKey, $"{row.CanonicalCode}:{row.DominantMethod}", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: method key mismatch for {row.CanonicalCode}.");
            }

            var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.BaselineMethod);
            if (!string.Equals(snapshot.BaselineProfileKey, baselineRoute?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineVariantKind, baselineRoute?.VariantKind, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineAlgorithmFamily, baselineRoute?.AlgorithmFamily, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineLegacyEnumName, baselineRoute?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineLegacyFormName, baselineRoute?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineLegacyAlgorithmEntry, baselineRoute?.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineLegacyMethodName, baselineRoute?.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineLegacySettingsMethodName, baselineRoute?.LegacySettingsMethodName, StringComparison.Ordinal)
                || !snapshot.BaselineSourceSections.SequenceEqual(GetSourceSections(baselineRoute), StringComparer.Ordinal)
                || !snapshot.BaselineSourceRanges.SequenceEqual(GetSourceRanges(baselineRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.BaselinePrimarySourceSection, GetPrimarySourceSection(baselineRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselinePrimarySourceRange, GetPrimarySourceRange(baselineRoute), StringComparison.Ordinal)
                || !snapshot.BaselineFormNames.SequenceEqual(GetFormNames(baselineRoute), StringComparer.Ordinal)
                || !snapshot.BaselineFormSourceRanges.SequenceEqual(GetFormSourceRanges(baselineRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.BaselinePrimaryFormName, GetPrimaryFormName(baselineRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselinePrimaryFormSourceRange, GetPrimaryFormSourceRange(baselineRoute), StringComparison.Ordinal)
                || snapshot.BaselineIsBaselineMethod != (baselineRoute?.IsBaselineMethod == true))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: baseline route projection mismatch for {row.CanonicalCode}.");
            }

            var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.DominantMethod);
            if (!string.Equals(snapshot.DominantProfileKey, dominantRoute?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantVariantKind, dominantRoute?.VariantKind, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantAlgorithmFamily, dominantRoute?.AlgorithmFamily, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyEnumName, dominantRoute?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyFormName, dominantRoute?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyAlgorithmEntry, dominantRoute?.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacyMethodName, dominantRoute?.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantLegacySettingsMethodName, dominantRoute?.LegacySettingsMethodName, StringComparison.Ordinal)
                || !snapshot.DominantSourceSections.SequenceEqual(GetSourceSections(dominantRoute), StringComparer.Ordinal)
                || !snapshot.DominantSourceRanges.SequenceEqual(GetSourceRanges(dominantRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.DominantPrimarySourceSection, GetPrimarySourceSection(dominantRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantPrimarySourceRange, GetPrimarySourceRange(dominantRoute), StringComparison.Ordinal)
                || !snapshot.DominantFormNames.SequenceEqual(GetFormNames(dominantRoute), StringComparer.Ordinal)
                || !snapshot.DominantFormSourceRanges.SequenceEqual(GetFormSourceRanges(dominantRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.DominantPrimaryFormName, GetPrimaryFormName(dominantRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantPrimaryFormSourceRange, GetPrimaryFormSourceRange(dominantRoute), StringComparison.Ordinal)
                || snapshot.DominantIsBaselineMethod != (dominantRoute?.IsBaselineMethod == true))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: dominant route projection mismatch for {row.CanonicalCode}.");
            }

            var expectedShare = row.TotalCount <= 0
                ? 0d
                : Math.Round((double)row.DominantCount / row.TotalCount, 4, MidpointRounding.AwayFromZero);
            if (Math.Abs(snapshot.DominantShare - expectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: dominant share mismatch for {row.CanonicalCode}. expected={expectedShare}, actual={snapshot.DominantShare}");
            }

            var expectedBaselineShare = row.TotalCount <= 0
                ? 0d
                : Math.Round((double)row.BaselineCount / row.TotalCount, 4, MidpointRounding.AwayFromZero);
            if (Math.Abs(snapshot.BaselineShare - expectedBaselineShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: baseline share mismatch for {row.CanonicalCode}. expected={expectedBaselineShare}, actual={snapshot.BaselineShare}");
            }

            var shouldPrioritizeDominant = row.BaselineMethod != row.DominantMethod
                && expectedShare >= DominantOverrideThreshold;
            if (snapshot.ShouldPrioritizeDominantOverBaseline != shouldPrioritizeDominant)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: prioritize flag mismatch for {row.CanonicalCode}.");
            }

            var decision = service.ListMotorYMethodDecisions().First(x => string.Equals(x.CanonicalCode, row.CanonicalCode, StringComparison.Ordinal));
            var recommendedRoute = decision.RecommendedRoute;
            var expectedLegacyBusinessCodes = new[]
            {
                recommendedRoute?.LegacyMethodName,
                dominantRoute?.LegacyMethodName,
                baselineRoute?.LegacyMethodName
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
            if (!snapshot.LegacyBusinessCodes.SequenceEqual(expectedLegacyBusinessCodes, StringComparer.Ordinal)
                || snapshot.LegacyBusinessCodes.Count == 0)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: legacy business code projection mismatch for {row.CanonicalCode}. expected={string.Join('/', expectedLegacyBusinessCodes)}, actual={string.Join('/', snapshot.LegacyBusinessCodes)}");
            }
            if ((snapshot.RecommendedMethod != (recommendedRoute?.MethodValue ?? 0))
                || !string.Equals(snapshot.RecommendedMethodKey, recommendedRoute?.MethodKey ?? string.Empty, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedProfileKey, recommendedRoute?.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedVariantKind, recommendedRoute?.VariantKind, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedAlgorithmFamily, recommendedRoute?.AlgorithmFamily, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedLegacyEnumName, recommendedRoute?.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedLegacyFormName, recommendedRoute?.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedLegacyAlgorithmEntry, recommendedRoute?.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedLegacyMethodName, recommendedRoute?.LegacyMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedLegacySettingsMethodName, recommendedRoute?.LegacySettingsMethodName, StringComparison.Ordinal)
                || !snapshot.RecommendedSourceSections.SequenceEqual(GetSourceSections(recommendedRoute), StringComparer.Ordinal)
                || !snapshot.RecommendedSourceRanges.SequenceEqual(GetSourceRanges(recommendedRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.RecommendedPrimarySourceSection, GetPrimarySourceSection(recommendedRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedPrimarySourceRange, GetPrimarySourceRange(recommendedRoute), StringComparison.Ordinal)
                || !snapshot.RecommendedFormNames.SequenceEqual(GetFormNames(recommendedRoute), StringComparer.Ordinal)
                || !snapshot.RecommendedFormSourceRanges.SequenceEqual(GetFormSourceRanges(recommendedRoute), StringComparer.Ordinal)
                || !string.Equals(snapshot.RecommendedPrimaryFormName, GetPrimaryFormName(recommendedRoute), StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedPrimaryFormSourceRange, GetPrimaryFormSourceRange(recommendedRoute), StringComparison.Ordinal)
                || snapshot.RecommendedIsBaselineMethod != (recommendedRoute?.IsBaselineMethod == true)
                || snapshot.RecommendedIsDominantMethod != string.Equals(recommendedRoute?.MethodKey, dominantRoute?.MethodKey, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedStrategy, decision.RecommendedStrategy, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendationReason, decision.RecommendationReason, StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedMethodSummary, decision.RecommendedMethodSummary, StringComparison.Ordinal)
                || !string.Equals(snapshot.BaselineDominantComparisonSummary, decision.BaselineDominantComparisonSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: recommended route projection mismatch for {row.CanonicalCode}.");
            }
        }
    }

    private static void AssertExplicitMethodDistributionBaseline(MotorYMethodRecommendationSnapshot snapshot)
    {
        if (string.Equals(snapshot.CanonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal))
        {
            if (snapshot.TotalCount != 87
                || snapshot.BaselineMethod != 4
                || snapshot.BaselineCount != 24
                || snapshot.DominantMethod != 60
                || snapshot.DominantCount != 61
                || !string.Equals(snapshot.DominantProfileKey, "MotorY.LoadA:60", StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantVariantKind, "delivery", StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedStrategy, "dominant-threshold-over-baseline", StringComparison.Ordinal)
                || snapshot.RecommendedMethod != 60)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: explicit LoadA baseline lock mismatch. total={snapshot.TotalCount}, baseline={snapshot.BaselineMethod}:{snapshot.BaselineCount}, dominant={snapshot.DominantMethod}:{snapshot.DominantCount}, recommended={snapshot.RecommendedMethod}:{snapshot.RecommendedStrategy}:{snapshot.DominantProfileKey}:{snapshot.DominantVariantKind}");
            }
        }

        if (string.Equals(snapshot.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
        {
            if (snapshot.TotalCount != 265
                || snapshot.BaselineMethod != 5
                || snapshot.BaselineCount != 233
                || snapshot.DominantMethod != 5
                || snapshot.DominantCount != 233
                || !string.Equals(snapshot.DominantProfileKey, "MotorY.LoadB:5", StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantVariantKind, "baseline", StringComparison.Ordinal)
                || !string.Equals(snapshot.RecommendedStrategy, "baseline", StringComparison.Ordinal)
                || snapshot.RecommendedMethod != 5)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: explicit LoadB baseline lock mismatch. total={snapshot.TotalCount}, baseline={snapshot.BaselineMethod}:{snapshot.BaselineCount}, dominant={snapshot.DominantMethod}:{snapshot.DominantCount}, recommended={snapshot.RecommendedMethod}:{snapshot.RecommendedStrategy}:{snapshot.DominantProfileKey}:{snapshot.DominantVariantKind}");
            }
        }

        if (string.Equals(snapshot.CanonicalCode, MotorYTestMethodCodes.LockedRotor, StringComparison.Ordinal))
        {
            if (snapshot.TotalCount != 7
                || snapshot.BaselineMethod != 11
                || snapshot.BaselineCount != 5
                || snapshot.DominantMethod != 11
                || snapshot.DominantCount != 5
                || !string.Equals(snapshot.DominantProfileKey, "MotorY.LockedRotor:11", StringComparison.Ordinal)
                || !string.Equals(snapshot.DominantVariantKind, "baseline", StringComparison.Ordinal)
                || snapshot.RecommendedMethod != 11
                || !string.Equals(snapshot.RecommendedStrategy, "baseline", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: explicit LockedRotor baseline lock mismatch. total={snapshot.TotalCount}, baseline={snapshot.BaselineMethod}:{snapshot.BaselineCount}, dominant={snapshot.DominantMethod}:{snapshot.DominantCount}, recommended={snapshot.RecommendedMethod}:{snapshot.RecommendedStrategy}:{snapshot.DominantProfileKey}:{snapshot.DominantVariantKind}");
            }
        }
    }

    private static (string CanonicalCode, int TotalCount, int BaselineMethod, int BaselineCount, int DominantMethod, int DominantCount) BuildExpected(
        SqliteConnection connection,
        string canonicalCode,
        int baselineMethod)
    {
        var rows = LoadCounts(connection, canonicalCode);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method recommendation smoke test failed: no source rows for {canonicalCode}.");
        }

        var baselineCount = rows.TryGetValue(baselineMethod, out var baseline) ? baseline : 0;
        var dominant = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .First();

        return (canonicalCode, rows.Values.Sum(), baselineMethod, baselineCount, dominant.Key, dominant.Value);
    }

    private static IReadOnlyList<string> GetSourceSections(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .SourceEvidences
            .Where(x => string.Equals(x.MethodName, route?.LegacyAlgorithmEntry, StringComparison.Ordinal))
            .Select(x => x.SectionKey)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> GetSourceRanges(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .SourceEvidences
            .Where(x => string.Equals(x.MethodName, route?.LegacyAlgorithmEntry, StringComparison.Ordinal))
            .Select(x => x.SourceRange)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static string GetPrimarySourceSection(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .SourceEvidences
            .Where(x => string.Equals(x.MethodName, route?.LegacyAlgorithmEntry, StringComparison.Ordinal))
            .GroupBy(x => x.SectionKey, StringComparer.Ordinal)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? string.Empty;

    private static string GetPrimarySourceRange(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .SourceEvidences
            .Where(x => string.Equals(x.MethodName, route?.LegacyAlgorithmEntry, StringComparison.Ordinal))
            .GroupBy(x => x.SourceRange, StringComparer.Ordinal)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? string.Empty;

    private static IReadOnlyList<string> GetFormNames(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .FormDependencyEvidences
            .Where(x => x.ReferencedMethods.Contains(route?.LegacyMethodName ?? string.Empty, StringComparer.Ordinal)
                || x.ReferencedMethods.Contains(route?.LegacySettingsMethodName ?? string.Empty, StringComparer.Ordinal))
            .Select(x => x.FormName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> GetFormSourceRanges(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .FormDependencyEvidences
            .Where(x => x.ReferencedMethods.Contains(route?.LegacyMethodName ?? string.Empty, StringComparer.Ordinal)
                || x.ReferencedMethods.Contains(route?.LegacySettingsMethodName ?? string.Empty, StringComparer.Ordinal))
            .Select(x => x.SourceRange)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

    private static string GetPrimaryFormName(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .FormDependencyEvidences
            .Where(x => x.ReferencedMethods.Contains(route?.LegacyMethodName ?? string.Empty, StringComparer.Ordinal)
                || x.ReferencedMethods.Contains(route?.LegacySettingsMethodName ?? string.Empty, StringComparer.Ordinal))
            .GroupBy(x => x.FormName, StringComparer.Ordinal)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? string.Empty;

    private static string GetPrimaryFormSourceRange(MotorYLegacyAlgorithmRoute? route)
        => MotorYLegacyAlgorithmDependencyCatalog.Get(route?.CanonicalCode ?? string.Empty)
            .FormDependencyEvidences
            .Where(x => x.ReferencedMethods.Contains(route?.LegacyMethodName ?? string.Empty, StringComparer.Ordinal)
                || x.ReferencedMethods.Contains(route?.LegacySettingsMethodName ?? string.Empty, StringComparer.Ordinal))
            .GroupBy(x => x.SourceRange, StringComparer.Ordinal)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? string.Empty;

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
