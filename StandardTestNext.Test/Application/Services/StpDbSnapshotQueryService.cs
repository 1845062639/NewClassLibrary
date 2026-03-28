using System.Text.Json;
using Microsoft.Data.Sqlite;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class StpDbSnapshotQueryService
{
    private const double MotorYDominantOverrideThreshold = 0.7d;

    private static readonly string[] MotorYLegacyItemCodes =
    [
        "直流电阻测定",
        "空载试验",
        "空载特性试验",
        "热试验",
        "A法负载试验",
        "B法负载试验",
        "堵转试验",
        "堵转特性试验"
    ];

    private readonly string _dbPath;

    public StpDbSnapshotQueryService(string? dbPath = null)
    {
        _dbPath = dbPath ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));
    }

    public IReadOnlyList<StpDbMotorYRecordSnapshot> ListRecentMotorYRecords(int take = 5)
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        var records = LoadRecentMotorYRecords(connection, take);
        if (records.Count == 0)
        {
            return Array.Empty<StpDbMotorYRecordSnapshot>();
        }

        var productTypeIds = records
            .Select(record => record.TestProductTypeId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var productTypes = LoadProductTypes(connection, productTypeIds);

        var recordIds = records.Select(record => record.Id).ToArray();
        var recordAttachmentMap = LoadRecordAttachments(connection, recordIds);
        records = records
            .Select(record => new StpDbTestRecordSnapshot
            {
                Id = record.Id,
                Code = record.Code,
                SerialNumber = record.SerialNumber,
                TestProductTypeId = record.TestProductTypeId,
                AccompanyProductTypeId = record.AccompanyProductTypeId,
                Kind = record.Kind,
                OwnDepart = record.OwnDepart,
                TestDepartId = record.TestDepartId,
                TesterId = record.TesterId,
                Remark = record.Remark,
                TestTimeRaw = record.TestTimeRaw,
                IsValid = record.IsValid,
                CreateTimeRaw = record.CreateTimeRaw,
                CreateBy = record.CreateBy,
                UpdateTimeRaw = record.UpdateTimeRaw,
                UpdateBy = record.UpdateBy,
                Attachments = recordAttachmentMap.TryGetValue(record.Id, out var attachments)
                    ? attachments
                    : Array.Empty<StpDbFileAttachmentSnapshot>()
            })
            .ToArray();

        var itemAttachmentMap = LoadMotorYItemAttachments(connection, recordIds);
        var itemsByRecordId = LoadMotorYItems(connection, recordIds, itemAttachmentMap)
            .GroupBy(item => item.TestRecordId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<StpDbTestRecordItemSnapshot>)group.OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase).ToArray(), StringComparer.OrdinalIgnoreCase);

        return records
            .Select(record => new StpDbMotorYRecordSnapshot
            {
                Record = record,
                ProductType = !string.IsNullOrWhiteSpace(record.TestProductTypeId) && productTypes.TryGetValue(record.TestProductTypeId, out var productType)
                    ? productType
                    : null,
                Items = itemsByRecordId.TryGetValue(record.Id, out var items)
                    ? items
                    : Array.Empty<StpDbTestRecordItemSnapshot>()
            })
            .ToArray();
    }

    public IReadOnlyList<StpDbMotorYMethodDistributionSnapshot> ListMotorYMethodDistribution()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorYMethodDistribution(connection);
    }

    public IReadOnlyList<StpDbMotorYLegacyCodeDistributionSnapshot> ListMotorYLegacyCodeDistribution()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorYLegacyCodeDistribution(connection);
    }

    public IReadOnlyList<StpDbMotorRatedParamsValueDistributionSnapshot> ListMotorRatedParamsValueDistribution()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorRatedParamsValueDistribution(connection);
    }

    public IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> ListMotorYMethodAdaptationPlans()
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        return LoadMotorYMethodAdaptationPlans(connection);
    }

    public IReadOnlyList<MotorYMethodRouteSelectionSnapshot> ListMotorYMethodRouteSelections()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorYMethodRouteSelections(connection);
    }

    public IReadOnlyList<MotorYMethodRecommendationSnapshot> ListMotorYMethodRecommendations()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorYMethodRecommendations(connection);
    }

    public IReadOnlyList<MotorYMethodDecisionSnapshot> ListMotorYMethodDecisions()
    {
        if (!File.Exists(_dbPath))
        {
            throw new InvalidOperationException($"stp.db not found: {_dbPath}");
        }

        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        return LoadMotorYMethodDecisions(connection);
    }

    private static IReadOnlyList<StpDbTestRecordSnapshot> LoadRecentMotorYRecords(SqliteConnection connection, int take)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT DISTINCT
    tr.ID,
    tr.Code,
    tr.SerialNumber,
    tr.TestProductTypeId,
    tr.AccompanyProductTypeId,
    tr.Kind,
    tr.OwnDepart,
    tr.TestDepartId,
    tr.TesterId,
    tr.Remark,
    tr.TestTime,
    tr.IsValid,
    tr.CreateTime,
    tr.CreateBy,
    tr.UpdateTime,
    tr.UpdateBy
FROM TestRecords tr
INNER JOIN TestRecordItems tri ON tri.TestRecordId = tr.ID
WHERE tri.Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)})
ORDER BY COALESCE(tr.TestTime, tr.CreateTime, tr.UpdateTime) DESC, tr.ID DESC
LIMIT $take;";
        command.Parameters.AddWithValue("$take", Math.Max(1, take));

        var items = new List<StpDbTestRecordSnapshot>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new StpDbTestRecordSnapshot
            {
                Id = reader.GetString(0),
                Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                SerialNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                TestProductTypeId = reader.IsDBNull(3) ? null : reader.GetString(3),
                AccompanyProductTypeId = reader.IsDBNull(4) ? null : reader.GetString(4),
                Kind = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                OwnDepart = reader.IsDBNull(6) ? null : reader.GetString(6),
                TestDepartId = reader.IsDBNull(7) ? null : reader.GetString(7),
                TesterId = reader.IsDBNull(8) ? null : reader.GetString(8),
                Remark = reader.IsDBNull(9) ? null : reader.GetString(9),
                TestTimeRaw = reader.IsDBNull(10) ? null : reader.GetString(10),
                IsValid = !reader.IsDBNull(11) && reader.GetInt64(11) == 1,
                CreateTimeRaw = reader.IsDBNull(12) ? null : reader.GetString(12),
                CreateBy = reader.IsDBNull(13) ? null : reader.GetString(13),
                UpdateTimeRaw = reader.IsDBNull(14) ? null : reader.GetString(14),
                UpdateBy = reader.IsDBNull(15) ? null : reader.GetString(15)
            });
        }

        return items;
    }

    private static Dictionary<string, StpDbProductTypeSnapshot> LoadProductTypes(
        SqliteConnection connection,
        IReadOnlyCollection<string> productTypeIds)
    {
        var result = new Dictionary<string, StpDbProductTypeSnapshot>(StringComparer.OrdinalIgnoreCase);
        if (productTypeIds.Count == 0)
        {
            return result;
        }

        using var command = connection.CreateCommand();
        var parameterNames = new List<string>();
        var index = 0;
        foreach (var productTypeId in productTypeIds)
        {
            var parameterName = "$p" + index++;
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, productTypeId);
        }

        command.CommandText = $@"
SELECT ID, COALESCE(Code, ''), COALESCE(RatedParams, '{{}}'), Category, Manufacturer, Remark, IsValid, CreateTime, CreateBy, UpdateTime, UpdateBy
FROM ProductTypes
WHERE ID IN ({string.Join(", ", parameterNames)});";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ratedParamsJson = reader.GetString(2);
            var snapshot = new StpDbProductTypeSnapshot
            {
                Id = reader.GetString(0),
                Code = reader.GetString(1),
                RatedParamsJson = ratedParamsJson,
                RatedParams = TryParseMotorRatedParams(reader.GetString(1), ratedParamsJson),
                Category = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Manufacturer = reader.IsDBNull(4) ? null : reader.GetString(4),
                Remark = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsValid = !reader.IsDBNull(6) && reader.GetInt64(6) == 1,
                CreateTimeRaw = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreateBy = reader.IsDBNull(8) ? null : reader.GetString(8),
                UpdateTimeRaw = reader.IsDBNull(9) ? null : reader.GetString(9),
                UpdateBy = reader.IsDBNull(10) ? null : reader.GetString(10)
            };

            result[snapshot.Id] = snapshot;
        }

        return result;
    }

    private static Dictionary<string, IReadOnlyList<StpDbFileAttachmentSnapshot>> LoadRecordAttachments(
        SqliteConnection connection,
        IReadOnlyCollection<string> recordIds)
    {
        var result = new Dictionary<string, IReadOnlyList<StpDbFileAttachmentSnapshot>>(StringComparer.OrdinalIgnoreCase);
        if (recordIds.Count == 0)
        {
            return result;
        }

        using var command = connection.CreateCommand();
        var parameterNames = new List<string>();
        var index = 0;
        foreach (var recordId in recordIds)
        {
            var parameterName = "$r" + index++;
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, recordId);
        }

        command.CommandText = $@"
SELECT
    tra.TestRecordId,
    fa.ID,
    COALESCE(fa.FileName, ''),
    COALESCE(fa.FileExt, ''),
    fa.Path,
    COALESCE(fa.Length, 0),
    fa.UploadTime,
    fa.SaveMode,
    fa.ExtraInfo,
    fa.HandlerInfo,
    COALESCE(tra.[Order], 0)
FROM TestRecordAttachments tra
INNER JOIN FileAttachments fa ON fa.ID = tra.FileId
WHERE tra.TestRecordId IN ({string.Join(", ", parameterNames)})
ORDER BY tra.TestRecordId, COALESCE(tra.[Order], 0), fa.UploadTime, fa.ID;";

        var grouped = new Dictionary<string, List<StpDbFileAttachmentSnapshot>>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var recordId = reader.GetString(0);
            if (!grouped.TryGetValue(recordId, out var attachments))
            {
                attachments = new List<StpDbFileAttachmentSnapshot>();
                grouped[recordId] = attachments;
            }

            attachments.Add(new StpDbFileAttachmentSnapshot
            {
                Id = reader.GetString(1),
                FileName = reader.GetString(2),
                FileExt = reader.GetString(3),
                Path = reader.IsDBNull(4) ? null : reader.GetString(4),
                Length = reader.GetInt64(5),
                UploadTimeRaw = reader.IsDBNull(6) ? null : reader.GetString(6),
                SaveMode = reader.IsDBNull(7) ? null : reader.GetString(7),
                ExtraInfo = reader.IsDBNull(8) ? null : reader.GetString(8),
                HandlerInfo = reader.IsDBNull(9) ? null : reader.GetString(9)
            });
        }

        foreach (var pair in grouped)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static IReadOnlyList<StpDbMotorYMethodDistributionSnapshot> LoadMotorYMethodDistribution(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COALESCE(Code, ''), Method, COUNT(*)
FROM TestRecordItems
WHERE Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)})
  AND Method IS NOT NULL
GROUP BY COALESCE(Code, ''), Method
ORDER BY COALESCE(Code, ''), Method;";

        var grouped = new Dictionary<(string CanonicalCode, int Method), int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            var method = reader.GetInt32(1);
            var count = reader.GetInt32(2);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            var key = (canonicalCode, method);
            grouped[key] = grouped.TryGetValue(key, out var currentCount)
                ? currentCount + count
                : count;
        }

        var snapshots = new List<StpDbMotorYMethodDistributionSnapshot>();
        foreach (var pair in grouped)
        {
            var route = MotorYLegacyAlgorithmRouteResolver.Resolve(pair.Key.CanonicalCode, pair.Key.Method);
            snapshots.Add(new StpDbMotorYMethodDistributionSnapshot
            {
                CanonicalCode = pair.Key.CanonicalCode,
                Method = pair.Key.Method,
                Count = pair.Value,
                MethodKey = $"{pair.Key.CanonicalCode}:{pair.Key.Method}",
                MethodProfileKey = route?.ProfileKey,
                VariantKind = route?.VariantKind,
                AlgorithmFamily = route?.AlgorithmFamily,
                LegacyEnumName = route?.LegacyEnumName,
                LegacyFormName = route?.LegacyFormName,
                LegacyAlgorithmEntry = route?.LegacyAlgorithmEntry,
                LegacyMethodName = route?.LegacyMethodName,
                LegacySettingsMethodName = route?.LegacySettingsMethodName,
                IsBaselineMethod = route?.IsBaselineMethod == true
            });
        }

        return snapshots
            .OrderBy(snapshot => snapshot.CanonicalCode, StringComparer.Ordinal)
            .ThenByDescending(snapshot => snapshot.Count)
            .ThenBy(snapshot => snapshot.Method)
            .ToArray();
    }

    private static IReadOnlyList<StpDbMotorYLegacyCodeDistributionSnapshot> LoadMotorYLegacyCodeDistribution(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COALESCE(Code, ''), Method, COUNT(*)
FROM TestRecordItems
WHERE Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)})
GROUP BY COALESCE(Code, ''), Method
ORDER BY COALESCE(Code, ''), Method;";

        var snapshots = new List<StpDbMotorYLegacyCodeDistributionSnapshot>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            var method = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
            snapshots.Add(new StpDbMotorYLegacyCodeDistributionSnapshot
            {
                CanonicalCode = canonicalCode,
                LegacyCode = legacyCode,
                Method = method,
                Count = reader.GetInt32(2),
                MethodKey = method.HasValue ? $"{canonicalCode}:{method.Value}" : $"{canonicalCode}:null",
                Route = method.HasValue ? MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, method.Value) : null
            });
        }

        return snapshots
            .OrderBy(row => row.CanonicalCode, StringComparer.Ordinal)
            .ThenByDescending(row => row.Count)
            .ThenBy(row => row.LegacyCode, StringComparer.Ordinal)
            .ThenBy(row => row.Method ?? int.MinValue)
            .ToArray();
    }

    private static IReadOnlyList<StpDbMotorRatedParamsValueDistributionSnapshot> LoadMotorRatedParamsValueDistribution(SqliteConnection connection)
    {
        var snapshots = new List<StpDbMotorRatedParamsValueDistributionSnapshot>();
        snapshots.AddRange(LoadRatedParamsFieldDistribution(connection, "Duty", NormalizeDuty));
        snapshots.AddRange(LoadRatedParamsFieldDistribution(connection, "Connection", NormalizeConnection));

        return snapshots
            .OrderBy(snapshot => snapshot.FieldName, StringComparer.Ordinal)
            .ThenByDescending(snapshot => snapshot.Count)
            .ThenBy(snapshot => snapshot.RawValue, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> LoadMotorYMethodAdaptationPlans(SqliteConnection connection)
    {
        return LoadMotorYMethodDecisions(connection)
            .Select(decision =>
            {
                var selectedRoute = decision.RecommendedRoute;
                var selectedCount = decision.ShouldPrioritizeDominantOverBaseline
                    ? decision.DominantCount
                    : decision.BaselineCount;
                var baselineShare = decision.TotalCount <= 0
                    ? 0d
                    : Math.Round((double)decision.BaselineCount / decision.TotalCount, 4, MidpointRounding.AwayFromZero);
                var dominantLeadCount = Math.Max(0, decision.DominantCount - decision.BaselineCount);
                var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((decision.DominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));

                return new MotorYMethodAdaptationPlanSnapshot
                {
                    CanonicalCode = decision.CanonicalCode,
                    TotalCount = decision.TotalCount,
                    BaselineRoute = decision.BaselineRoute,
                    BaselineCount = decision.BaselineCount,
                    DominantRoute = decision.DominantRoute,
                    DominantCount = decision.DominantCount,
                    SelectedRoute = selectedRoute,
                    SelectedCount = selectedCount,
                    SelectionStrategy = decision.RecommendedStrategy,
                    ShouldUseDominantRoute = decision.ShouldPrioritizeDominantOverBaseline,
                    DominantShare = decision.DominantShare,
                    DominantOverrideThreshold = decision.DominantOverrideThreshold,
                    DominantLeadCount = dominantLeadCount,
                    DominantLeadPercentagePoints = dominantLeadPercentagePoints,
                    SelectionReason = decision.RecommendationReason,
                    AlgorithmEntry = selectedRoute?.LegacyAlgorithmEntry ?? string.Empty,
                    SettingsMethodName = selectedRoute?.LegacySettingsMethodName ?? string.Empty,
                    LegacyMethodName = selectedRoute?.LegacyMethodName ?? string.Empty,
                    Distributions = decision.Distributions
                };
            })
            .ToArray();
    }

    private static IReadOnlyList<MotorYMethodRouteSelectionSnapshot> LoadMotorYMethodRouteSelections(SqliteConnection connection)
    {
        return LoadMotorYMethodDecisions(connection)
            .Select(MotorYMethodRouteSelectionSnapshotFactory.Create)
            .OrderBy(snapshot => snapshot.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<MotorYMethodRecommendationSnapshot> LoadMotorYMethodRecommendations(SqliteConnection connection)
    {
        return LoadMotorYMethodDecisions(connection)
            .Select(decision => new MotorYMethodRecommendationSnapshot
            {
                CanonicalCode = decision.CanonicalCode,
                TotalCount = decision.TotalCount,
                BaselineMethod = decision.BaselineRoute?.MethodValue ?? 0,
                BaselineCount = decision.BaselineCount,
                BaselineMethodKey = decision.BaselineRoute?.MethodKey ?? string.Empty,
                BaselineProfileKey = decision.BaselineRoute?.ProfileKey,
                DominantMethod = decision.DominantRoute?.MethodValue ?? 0,
                DominantCount = decision.DominantCount,
                DominantMethodKey = decision.DominantRoute?.MethodKey ?? string.Empty,
                DominantProfileKey = decision.RecommendedRoute?.ProfileKey,
                DominantVariantKind = decision.RecommendedRoute?.VariantKind,
                DominantAlgorithmFamily = decision.RecommendedRoute?.AlgorithmFamily,
                DominantLegacyEnumName = decision.RecommendedRoute?.LegacyEnumName,
                DominantLegacyFormName = decision.RecommendedRoute?.LegacyFormName,
                DominantLegacyAlgorithmEntry = decision.RecommendedRoute?.LegacyAlgorithmEntry,
                DominantLegacyMethodName = decision.RecommendedRoute?.LegacyMethodName,
                DominantLegacySettingsMethodName = decision.RecommendedRoute?.LegacySettingsMethodName,
                DominantIsBaselineMethod = decision.RecommendedRoute?.IsBaselineMethod == true,
                ShouldPrioritizeDominantOverBaseline = decision.ShouldPrioritizeDominantOverBaseline,
                DominantShare = decision.DominantShare
            })
            .ToArray();
    }

    private static IReadOnlyList<MotorYMethodDecisionSnapshot> LoadMotorYMethodDecisions(SqliteConnection connection)
    {
        var distribution = LoadMotorYMethodDistribution(connection);
        var result = new List<MotorYMethodDecisionSnapshot>();

        foreach (var group in distribution.GroupBy(row => row.CanonicalCode, StringComparer.Ordinal))
        {
            var ordered = group
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.Method)
                .ToArray();
            if (ordered.Length == 0)
            {
                continue;
            }

            var dominant = ordered[0];
            var baseline = group.FirstOrDefault(row => row.IsBaselineMethod)
                ?? ordered.FirstOrDefault(row => string.Equals(row.MethodProfileKey, "baseline", StringComparison.Ordinal))
                ?? ordered.OrderBy(row => row.Method).First();
            var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(group.Key, baseline.Method);
            var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(group.Key, dominant.Method);
            var totalCount = group.Sum(row => row.Count);
            var dominantShare = totalCount <= 0
                ? 0d
                : Math.Round((double)dominant.Count / totalCount, 4, MidpointRounding.AwayFromZero);
            var baselineShare = totalCount <= 0
                ? 0d
                : Math.Round((double)baseline.Count / totalCount, 4, MidpointRounding.AwayFromZero);
            var dominantLeadCount = Math.Max(0, dominant.Count - baseline.Count);
            var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((dominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
            var shouldPrioritizeDominant = dominant.Method != baseline.Method
                && dominantShare >= MotorYDominantOverrideThreshold;
            var recommendedRoute = shouldPrioritizeDominant ? dominantRoute : baselineRoute;
            var recommendedStrategy = shouldPrioritizeDominant
                ? "dominant-threshold-over-baseline"
                : "baseline";

            result.Add(new MotorYMethodDecisionSnapshot
            {
                CanonicalCode = group.Key,
                TotalCount = totalCount,
                BaselineRoute = baselineRoute,
                BaselineCount = baseline.Count,
                DominantRoute = dominantRoute,
                DominantCount = dominant.Count,
                RecommendedRoute = recommendedRoute,
                RecommendedStrategy = recommendedStrategy,
                ShouldPrioritizeDominantOverBaseline = shouldPrioritizeDominant,
                DominantShare = dominantShare,
                BaselineShare = baselineShare,
                DominantOverrideThreshold = MotorYDominantOverrideThreshold,
                DominantLeadCount = dominantLeadCount,
                DominantLeadPercentagePoints = dominantLeadPercentagePoints,
                RecommendationReason = BuildMotorYMethodSelectionReason(
                    shouldPrioritizeDominant,
                    baseline.Method,
                    dominant.Method,
                    dominantShare,
                    baselineShare,
                    MotorYDominantOverrideThreshold,
                    dominantLeadCount,
                    dominantLeadPercentagePoints),
                Distributions = ordered
                    .Select(row => new MotorYMethodDistributionSnapshot
                    {
                        MethodValue = row.Method,
                        Count = row.Count,
                        Share = totalCount <= 0
                            ? 0d
                            : Math.Round((double)row.Count / totalCount, 4, MidpointRounding.AwayFromZero),
                        Route = MotorYLegacyAlgorithmRouteResolver.Resolve(group.Key, row.Method)
                    })
                    .ToArray()
            });
        }

        return result
            .OrderBy(snapshot => snapshot.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildMotorYMethodSelectionReason(
        bool shouldPrioritizeDominant,
        int baselineMethod,
        int dominantMethod,
        double dominantShare,
        double baselineShare,
        double threshold,
        int dominantLeadCount,
        int dominantLeadPercentagePoints)
    {
        if (shouldPrioritizeDominant)
        {
            return $"selected dominant method {dominantMethod} over baseline {baselineMethod} because dominant share {dominantShare:P2} reached threshold {threshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)";
        }

        if (baselineMethod == dominantMethod)
        {
            return $"kept baseline method {baselineMethod} because baseline already matches dominant distribution ({dominantShare:P2})";
        }

        return $"kept baseline method {baselineMethod} because dominant method {dominantMethod} share {dominantShare:P2} did not reach threshold {threshold:P0}";
    }

    private static IReadOnlyList<StpDbMotorRatedParamsValueDistributionSnapshot> LoadRatedParamsFieldDistribution(
        SqliteConnection connection,
        string fieldName,
        Func<string, string> normalizer)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT CAST(json_extract(RatedParams, '$.{fieldName}') AS TEXT) AS RawValue, COUNT(*)
FROM ProductTypes
GROUP BY CAST(json_extract(RatedParams, '$.{fieldName}') AS TEXT)
ORDER BY COUNT(*) DESC, RawValue;";

        var snapshots = new List<StpDbMotorRatedParamsValueDistributionSnapshot>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var rawValue = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            snapshots.Add(new StpDbMotorRatedParamsValueDistributionSnapshot
            {
                FieldName = fieldName,
                RawValue = rawValue,
                Count = reader.GetInt32(1),
                NormalizedValue = normalizer(rawValue)
            });
        }

        return snapshots;
    }

    private static IReadOnlyList<StpDbTestRecordItemSnapshot> LoadMotorYItems(
        SqliteConnection connection,
        IReadOnlyCollection<string> recordIds,
        IReadOnlyDictionary<string, IReadOnlyList<StpDbFileAttachmentSnapshot>> itemAttachmentMap)
    {
        if (recordIds.Count == 0)
        {
            return Array.Empty<StpDbTestRecordItemSnapshot>();
        }

        using var command = connection.CreateCommand();
        var parameterNames = new List<string>();
        var index = 0;
        foreach (var recordId in recordIds)
        {
            var parameterName = "$r" + index++;
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, recordId);
        }

        command.CommandText = $@"
SELECT ID, COALESCE(Code, ''), Method, COALESCE(Data, '{{}}'), Remark, TestRecordId, IsValid, CreateTime, CreateBy, UpdateTime, UpdateBy
FROM TestRecordItems
WHERE TestRecordId IN ({string.Join(", ", parameterNames)})
  AND Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)});";

        var items = new List<StpDbTestRecordItemSnapshot>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetString(0);
            var legacyCode = reader.GetString(1);
            int? method = reader.IsDBNull(2) ? null : reader.GetInt32(2);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            var methodKey = BuildMotorYMethodKey(legacyCode, method);
            var legacyRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, method);

            items.Add(new StpDbTestRecordItemSnapshot
            {
                Id = id,
                Code = legacyCode,
                Method = method,
                CanonicalCode = canonicalCode,
                MethodKey = methodKey,
                MethodProfileKey = legacyRoute?.ProfileKey,
                VariantKind = legacyRoute?.VariantKind,
                AlgorithmFamily = legacyRoute?.AlgorithmFamily,
                LegacyAlgorithmRoute = legacyRoute,
                LegacyEnumName = legacyRoute?.LegacyEnumName,
                LegacyFormName = legacyRoute?.LegacyFormName,
                LegacyAlgorithmEntry = legacyRoute?.LegacyAlgorithmEntry,
                LegacyMethodName = legacyRoute?.LegacyMethodName,
                LegacySettingsMethodName = legacyRoute?.LegacySettingsMethodName,
                IsBaselineMethod = legacyRoute?.IsBaselineMethod == true,
                DataJson = reader.GetString(3),
                Remark = reader.IsDBNull(4) ? null : reader.GetString(4),
                TestRecordId = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsValid = !reader.IsDBNull(6) && reader.GetInt64(6) == 1,
                CreateTimeRaw = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreateBy = reader.IsDBNull(8) ? null : reader.GetString(8),
                UpdateTimeRaw = reader.IsDBNull(9) ? null : reader.GetString(9),
                UpdateBy = reader.IsDBNull(10) ? null : reader.GetString(10),
                Attachments = itemAttachmentMap.TryGetValue(id, out var attachments)
                    ? attachments
                    : Array.Empty<StpDbFileAttachmentSnapshot>()
            });
        }

        return items;
    }

    private static Dictionary<string, IReadOnlyList<StpDbFileAttachmentSnapshot>> LoadMotorYItemAttachments(
        SqliteConnection connection,
        IReadOnlyCollection<string> recordIds)
    {
        var result = new Dictionary<string, IReadOnlyList<StpDbFileAttachmentSnapshot>>(StringComparer.OrdinalIgnoreCase);
        if (recordIds.Count == 0)
        {
            return result;
        }

        using var command = connection.CreateCommand();
        var parameterNames = new List<string>();
        var index = 0;
        foreach (var recordId in recordIds)
        {
            var parameterName = "$r" + index++;
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, recordId);
        }

        command.CommandText = $@"
SELECT
    tri.ID,
    fa.ID,
    COALESCE(fa.FileName, ''),
    COALESCE(fa.FileExt, ''),
    fa.Path,
    COALESCE(fa.Length, 0),
    fa.UploadTime,
    fa.SaveMode,
    fa.ExtraInfo,
    fa.HandlerInfo,
    COALESCE(tria.[Order], 0)
FROM TestRecordItems tri
INNER JOIN TestRecordItemAttachments tria ON tria.TestRecordItemId = tri.ID
INNER JOIN FileAttachments fa ON fa.ID = tria.FileId
WHERE tri.TestRecordId IN ({string.Join(", ", parameterNames)})
  AND tri.Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)})
ORDER BY tri.ID, COALESCE(tria.[Order], 0), fa.UploadTime, fa.ID;";

        var grouped = new Dictionary<string, List<StpDbFileAttachmentSnapshot>>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var itemId = reader.GetString(0);
            if (!grouped.TryGetValue(itemId, out var attachments))
            {
                attachments = new List<StpDbFileAttachmentSnapshot>();
                grouped[itemId] = attachments;
            }

            attachments.Add(new StpDbFileAttachmentSnapshot
            {
                Id = reader.GetString(1),
                FileName = reader.GetString(2),
                FileExt = reader.GetString(3),
                Path = reader.IsDBNull(4) ? null : reader.GetString(4),
                Length = reader.GetInt64(5),
                UploadTimeRaw = reader.IsDBNull(6) ? null : reader.GetString(6),
                SaveMode = reader.IsDBNull(7) ? null : reader.GetString(7),
                ExtraInfo = reader.IsDBNull(8) ? null : reader.GetString(8),
                HandlerInfo = reader.IsDBNull(9) ? null : reader.GetString(9)
            });
        }

        foreach (var pair in grouped)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }

    private static MotorRatedParamsContract? TryParseMotorRatedParams(string productTypeCode, string ratedParamsJson)
    {
        if (string.IsNullOrWhiteSpace(ratedParamsJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(ratedParamsJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var pole = ReadInt32(root, "Pole");
            var polePairs = ReadInt32(root, "PolePairs");
            if (polePairs == 0 && pole > 0)
            {
                polePairs = Math.Max(1, pole / 2);
            }

            var rawDuty = ReadStringish(root, "Duty");
            var rawConnection = ReadStringish(root, "Connection");

            return new MotorRatedParamsContract
            {
                ProductKind = "Motor_Y",
                Model = productTypeCode,
                StandardCode = string.Empty,
                RatedPowerRaw = ReadDouble(root, "RatedPower"),
                RatedPower = NormalizeRatedPower(ReadDouble(root, "RatedPower")),
                RatedCurrent = ReadDouble(root, "RatedCurrent"),
                RatedVoltage = ReadDouble(root, "RatedVoltage"),
                RatedSpeed = ReadDouble(root, "RatedSpeed"),
                RatedFrequency = ReadDouble(root, "RatedFrequency"),
                Pole = pole,
                PolePairs = polePairs,
                Duty = NormalizeDuty(rawDuty),
                DutyRaw = rawDuty,
                InsulationGrade = ReadStringish(root, "InsulationGrade"),
                PowerFactor = ReadDouble(root, "PowerFactor"),
                Weight = ReadDouble(root, "Weight"),
                IngressProtection = ReadStringish(root, "IngressProtection"),
                Connection = NormalizeConnection(rawConnection),
                ConnectionRaw = rawConnection
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static double ReadDouble(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
        {
            return 0;
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.GetDouble();
        }

        if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), out var value))
        {
            return value;
        }

        return 0;
    }

    private static int ReadInt32(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
        {
            return 0;
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetInt32(out var value) ? value : (int)Math.Round(element.GetDouble());
        }

        if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    private static string ReadStringish(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element))
        {
            return string.Empty;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue.ToString() : element.GetDouble().ToString("0.####"),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => string.Empty
        };
    }

    private static double NormalizeRatedPower(double rawRatedPower)
    {
        if (rawRatedPower <= 0)
        {
            return 0;
        }

        return rawRatedPower >= 1000 ? Math.Round(rawRatedPower / 1000.0, 4) : Math.Round(rawRatedPower, 4);
    }

    private static string NormalizeDuty(string rawDuty)
    {
        return rawDuty switch
        {
            "0" => string.Empty,
            _ => rawDuty
        };
    }

    private static string NormalizeConnection(string rawConnection)
    {
        return rawConnection switch
        {
            "0" => "Y",
            "1" => "Δ",
            _ => rawConnection
        };
    }

    private static string BuildMotorYMethodKey(string legacyCode, int? method)
    {
        var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
        return string.IsNullOrWhiteSpace(canonicalCode)
            ? (method.HasValue ? $"Unknown:{method.Value}" : "Unknown")
            : method.HasValue ? $"{canonicalCode}:{method.Value}" : $"{canonicalCode}:<null>";
    }

    private static string BuildInlineQuotedList(IEnumerable<string> values)
    {
        return string.Join(", ", values.Select(value => $"'{value.Replace("'", "''")}'"));
    }
}
