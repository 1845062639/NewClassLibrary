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

    private static Dictionary<string, MotorYLegacyCodeSelectionSnapshot> LoadMotorYLegacyCodeSelections(SqliteConnection connection)
    {
        var distributions = LoadMotorYLegacyCodeDistribution(connection)
            .GroupBy(row => row.CanonicalCode, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var items = group
                        .GroupBy(row => row.LegacyCode, StringComparer.Ordinal)
                        .Select(codeGroup => new MotorYLegacyCodeDistributionSnapshot
                        {
                            CanonicalCode = group.Key,
                            LegacyCode = codeGroup.Key,
                            Count = codeGroup.Sum(x => x.Count),
                            Share = Math.Round((double)codeGroup.Sum(x => x.Count) / Math.Max(1, group.Sum(x => x.Count)), 4, MidpointRounding.AwayFromZero)
                        })
                        .OrderByDescending(x => x.Count)
                        .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
                        .ToArray();

                    var recommended = items.FirstOrDefault();
                    return new MotorYLegacyCodeSelectionSnapshot
                    {
                        CanonicalCode = group.Key,
                        RecommendedLegacyCode = recommended?.LegacyCode ?? string.Empty,
                        DominantLegacyCode = recommended?.LegacyCode ?? string.Empty,
                        RecommendedLegacyCodeCount = recommended?.Count ?? 0,
                        RecommendedLegacyCodeShare = recommended?.Share ?? 0d,
                        Distributions = items,
                        Summary = recommended is null
                            ? $"legacy code selection unavailable for {group.Key}"
                            : $"recommended legacy code '{recommended.LegacyCode}' for {group.Key} ({recommended.Count}/{group.Sum(x => x.Count)}, {(int)Math.Round(recommended.Share * 100d, MidpointRounding.AwayFromZero)}pp)"
                    };
                },
                StringComparer.Ordinal);

        return distributions;
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

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> LoadObservedLegacyCodesByCanonicalCode(SqliteConnection connection)
    {
        const string sql = @"
SELECT
    COALESCE(curr.Code, ''),
    COALESCE(upstream.Code, '')
FROM TestRecordItems curr
JOIN TestRecordItems upstream ON upstream.TestRecordId = curr.TestRecordId
WHERE COALESCE(curr.Code, '') <> ''
  AND COALESCE(upstream.Code, '') <> ''
  AND curr.TestRecordId IS NOT NULL;";

        var buckets = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.Ordinal);
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var currentCode = reader.GetString(0);
            var upstreamCode = reader.GetString(1);
            var currentCanonical = MotorYLegacyItemCodeNormalizer.Normalize(currentCode);
            var upstreamCanonical = MotorYLegacyItemCodeNormalizer.Normalize(upstreamCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(currentCanonical)
                || !MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(upstreamCanonical)
                || string.Equals(currentCanonical, upstreamCanonical, StringComparison.Ordinal))
            {
                continue;
            }

            if (!buckets.TryGetValue(currentCanonical, out var upstreamBuckets))
            {
                upstreamBuckets = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                buckets[currentCanonical] = upstreamBuckets;
            }

            if (!upstreamBuckets.TryGetValue(upstreamCanonical, out var observedCodes))
            {
                observedCodes = new List<string>();
                upstreamBuckets[upstreamCanonical] = observedCodes;
            }

            observedCodes.Add(upstreamCode);
        }

        return buckets.ToDictionary(
            current => current.Key,
            current => (IReadOnlyDictionary<string, IReadOnlyList<string>>)current.Value.ToDictionary(
                upstream => upstream.Key,
                upstream => (IReadOnlyList<string>)upstream.Value.ToArray(),
                StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    private static IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> LoadMotorYMethodAdaptationPlans(SqliteConnection connection)
    {
        var samplePayloads = LoadMotorYSamplePayloadByCanonicalCode(connection);
        var sampleRatedParams = LoadMotorYSampleRatedParamsByCanonicalCode(connection);
        var legacyCodeDistributions = LoadMotorYLegacyCodeSelections(connection);
        var observedLegacyCodesByCanonicalCode = LoadObservedLegacyCodesByCanonicalCode(connection);

        return LoadMotorYMethodDecisions(connection)
            .Select(decision =>
            {
                var selection = MotorYMethodRouteSelectionSnapshotFactory.Create(decision);
                var selectedRoute = selection.SelectedRoute;
                var dependencyProfile = MotorYLegacyAlgorithmDependencyCatalog.TryGet(selection.CanonicalCode);
                legacyCodeDistributions.TryGetValue(selection.CanonicalCode, out var legacyCodeSelection);
                observedLegacyCodesByCanonicalCode.TryGetValue(selection.CanonicalCode, out var upstreamObservedLegacyCodes);
                var upstreamLegacyAliases = dependencyProfile?.UpstreamLegacyAliases
                    ?? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
                var upstreamLegacyCodeDistributions = upstreamLegacyAliases
                    .OrderBy(x => x.Key, StringComparer.Ordinal)
                    .SelectMany(pair => MotorYLegacyUpstreamCodeCatalog.BuildDistributions(
                        pair.Key,
                        upstreamObservedLegacyCodes is not null && upstreamObservedLegacyCodes.TryGetValue(pair.Key, out var codes)
                            ? codes
                            : Array.Empty<string>()))
                    .ToArray();
                var requiredPayloadFields = dependencyProfile?.RequiredPayloadFields ?? Array.Empty<string>();
                var upstream = MotorYUpstreamDependencySnapshotFactory.Create(
                    selection.CanonicalCode,
                    dependencyProfile?.UpstreamCanonicalCodes ?? Array.Empty<string>(),
                    samplePayloads.Keys);
                samplePayloads.TryGetValue(selection.CanonicalCode, out var sampleDataJson);
                sampleRatedParams.TryGetValue(selection.CanonicalCode, out var sampleRatedParamsContract);
                var coverage = MotorYRequiredPayloadFieldCoverageEvaluator.Evaluate(
                    selection.CanonicalCode,
                    requiredPayloadFields,
                    sampleDataJson);
                var ratedCoverage = MotorYRequiredRatedParamFieldCoverageEvaluator.Evaluate(
                    selection.CanonicalCode,
                    dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
                    sampleRatedParamsContract);
                var rawDataSignalCoverage = MotorYRawDataSignalCoverageEvaluator.Evaluate(
                    selection.CanonicalCode,
                    sampleDataJson);
                var resultCoverage = MotorYRequiredResultFieldCoverageEvaluator.Evaluate(
                    selection.CanonicalCode,
                    dependencyProfile?.RequiredResultFields ?? Array.Empty<string>(),
                    sampleDataJson);
                var formulaEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildFormulaSignalEvidence(
                    selection.CanonicalCode,
                    resultCoverage.CoveredRequiredResultFields);
                var formulaCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
                    dependencyProfile?.FormulaSignals,
                    formulaEvidence.ObservedPayloadFields,
                    "formula signals");
                var ruleObservedFields = coverage.CoveredRequiredPayloadFields
                    .Concat(ratedCoverage.CoveredRequiredRatedParamFields)
                    .Concat(resultCoverage.CoveredRequiredResultFields)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var ruleEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildLegacyRuleEvidence(
                    selection.CanonicalCode,
                    ruleObservedFields);
                var ruleCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
                    dependencyProfile?.LegacyAlgorithmRules,
                    ruleEvidence.ObservedPayloadFields,
                    "legacy algorithm rules");
                var structuredPayloadCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
                    dependencyProfile?.RequiredStructuredPayloadSignals,
                    sampleDataJson,
                    "structured payload signals");
                var structuredResultCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
                    dependencyProfile?.RequiredStructuredResultSignals,
                    sampleDataJson,
                    "structured result signals");
                var rawDataSignalsReady = rawDataSignalCoverage.MissingSignals.Count == 0;
                var structuredSignalsReady = structuredPayloadCoverage.MissingSignalCount == 0
                    && structuredResultCoverage.MissingSignalCount == 0;
                var requiredResultFieldsReady = resultCoverage.MissingRequiredResultFieldCount == 0;
                var legacyAlgorithmInputsReady = upstream.UpstreamDependenciesSatisfied
                    && coverage.MissingRequiredPayloadFieldCount == 0
                    && ratedCoverage.MissingRequiredRatedParamFieldCount == 0
                    && requiredResultFieldsReady
                    && rawDataSignalsReady
                    && structuredSignalsReady;
                var legacyAlgorithmInputReadinessSummary = BuildLegacyAlgorithmInputReadinessSummary(
                    upstream,
                    coverage,
                    ratedCoverage,
                    resultCoverage,
                    rawDataSignalCoverage,
                    structuredPayloadCoverage,
                    structuredResultCoverage,
                    legacyAlgorithmInputsReady);
                var dependencyBuckets = MotorYDependencyBucketSummaryFactory.Create(
                    upstream,
                    coverage,
                    ratedCoverage,
                    resultCoverage,
                    rawDataSignalCoverage,
                    structuredPayloadCoverage,
                    structuredResultCoverage,
                    formulaCoverage,
                    ruleCoverage);

                return new MotorYMethodAdaptationPlanSnapshot
                {
                    CanonicalCode = selection.CanonicalCode,
                    TotalCount = selection.TotalCount,
                    BaselineRoute = selection.BaselineRoute,
                    BaselineCount = selection.BaselineCount,
                    BaselineShare = selection.BaselineShare,
                    DominantRoute = selection.DominantRoute,
                    DominantCount = selection.DominantCount,
                    DominantShare = selection.DominantShare,
                    SelectedRoute = selectedRoute,
                    SelectedCount = selection.SelectedCount,
                    SelectedShare = selection.SelectedShare,
                    SelectionStrategy = selection.SelectionStrategy,
                    ShouldUseDominantRoute = selection.ShouldUseDominantRoute,
                    DominantOverrideThreshold = selection.DominantOverrideThreshold,
                    DominantLeadCount = selection.DominantLeadCount,
                    DominantLeadPercentagePoints = selection.DominantLeadPercentagePoints,
                    SelectedLeadCountVsBaseline = selection.SelectedLeadCountVsBaseline,
                    SelectedLeadPercentagePointsVsBaseline = selection.SelectedLeadPercentagePointsVsBaseline,
                    SelectionReason = selection.SelectionReason,
                    AlgorithmEntry = selectedRoute?.LegacyAlgorithmEntry ?? string.Empty,
                    SettingsMethodName = selectedRoute?.LegacySettingsMethodName ?? string.Empty,
                    LegacyMethodName = selectedRoute?.LegacyMethodName ?? string.Empty,
                    RecommendedLegacyCode = legacyCodeSelection?.RecommendedLegacyCode ?? string.Empty,
                    DominantLegacyCode = legacyCodeSelection?.DominantLegacyCode ?? string.Empty,
                    RecommendedLegacyCodeCount = legacyCodeSelection?.RecommendedLegacyCodeCount ?? 0,
                    RecommendedLegacyCodeShare = legacyCodeSelection?.RecommendedLegacyCodeShare ?? 0d,
                    LegacyCodeSelectionSummary = legacyCodeSelection?.Summary ?? string.Empty,
                    LegacyCodeDistributions = legacyCodeSelection?.Distributions ?? Array.Empty<MotorYLegacyCodeDistributionSnapshot>(),
                    RequiresRatedParams = dependencyProfile?.RequiresRatedParams == true,
                    UpstreamCanonicalCodes = dependencyProfile?.UpstreamCanonicalCodes ?? Array.Empty<string>(),
                    UpstreamLegacyAliases = upstreamLegacyAliases,
                    UpstreamLegacyCodeDistributions = upstreamLegacyCodeDistributions,
                    ObservedUpstreamCanonicalCodeCount = upstream.ObservedUpstreamCanonicalCodeCount,
                    ObservedUpstreamCanonicalCodes = upstream.ObservedUpstreamCanonicalCodes,
                    MissingUpstreamCanonicalCodes = upstream.MissingUpstreamCanonicalCodes,
                    UpstreamDependenciesSatisfied = upstream.UpstreamDependenciesSatisfied,
                    UpstreamDependencySummary = upstream.UpstreamDependencySummary,
                    RequiredPayloadFields = requiredPayloadFields,
                    RequiredRatedParamFields = dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
                    RequiredResultFields = dependencyProfile?.RequiredResultFields ?? Array.Empty<string>(),
                    CoveredRequiredResultFieldCount = resultCoverage.CoveredRequiredResultFieldCount,
                    MissingRequiredResultFieldCount = resultCoverage.MissingRequiredResultFieldCount,
                    MissingRequiredResultFields = resultCoverage.MissingRequiredResultFields,
                    CoveredRequiredResultFields = resultCoverage.CoveredRequiredResultFields,
                    RequiredResultFieldCoverageRatio = resultCoverage.RequiredResultFieldCoverageRatio,
                    RequiredResultFieldCoveragePercentagePoints = resultCoverage.RequiredResultFieldCoveragePercentagePoints,
                    RequiredResultFieldCoverageSummary = resultCoverage.RequiredResultFieldCoverageSummary,
                    CoveredRequiredPayloadFieldCount = coverage.CoveredRequiredPayloadFieldCount,
                    MissingRequiredPayloadFieldCount = coverage.MissingRequiredPayloadFieldCount,
                    MissingRequiredPayloadFields = coverage.MissingRequiredPayloadFields,
                    CoveredRequiredPayloadFields = coverage.CoveredRequiredPayloadFields,
                    RequiredPayloadFieldCoverageRatio = coverage.RequiredPayloadFieldCoverageRatio,
                    RequiredPayloadFieldCoveragePercentagePoints = coverage.RequiredPayloadFieldCoveragePercentagePoints,
                    SamplePayloadAvailable = coverage.SamplePayloadAvailable,
                    RequiredPayloadFieldCoverageSummary = coverage.RequiredPayloadFieldCoverageSummary,
                    RequiredRawDataSignals = rawDataSignalCoverage.RequiredSignals,
                    ObservedRawDataSignals = rawDataSignalCoverage.ObservedSignals,
                    MissingRawDataSignals = rawDataSignalCoverage.MissingSignals,
                    RawDataSignalCoveredCount = rawDataSignalCoverage.ObservedSignals.Count,
                    RawDataSignalMissingCount = rawDataSignalCoverage.MissingSignals.Count,
                    RawDataSampleCount = rawDataSignalCoverage.RawSampleCount,
                    RawDataListAvailable = rawDataSignalCoverage.RawDataListAvailable,
                    RawDataSignalCoverageRatio = rawDataSignalCoverage.CoverageRatio,
                    RawDataSignalCoveragePercentagePoints = rawDataSignalCoverage.CoveragePercentagePoints,
                    RawDataSignalCoverageSummary = rawDataSignalCoverage.Summary,
                    CoveredRequiredRatedParamFieldCount = ratedCoverage.CoveredRequiredRatedParamFieldCount,
                    MissingRequiredRatedParamFieldCount = ratedCoverage.MissingRequiredRatedParamFieldCount,
                    MissingRequiredRatedParamFields = ratedCoverage.MissingRequiredRatedParamFields,
                    CoveredRequiredRatedParamFields = ratedCoverage.CoveredRequiredRatedParamFields,
                    RequiredRatedParamFieldCoverageRatio = ratedCoverage.RequiredRatedParamFieldCoverageRatio,
                    RequiredRatedParamFieldCoveragePercentagePoints = ratedCoverage.RequiredRatedParamFieldCoveragePercentagePoints,
                    RatedParamsAvailable = ratedCoverage.RatedParamsAvailable,
                    RequiredRatedParamFieldCoverageSummary = ratedCoverage.RequiredRatedParamFieldCoverageSummary,
                    LegacyAlgorithmInputsReady = legacyAlgorithmInputsReady,
                    RawDataSignalsReady = rawDataSignalsReady,
                    RequiredStructuredPayloadSignals = dependencyProfile?.RequiredStructuredPayloadSignals ?? Array.Empty<string>(),
                    ObservedStructuredPayloadSignals = structuredPayloadCoverage.ObservedSignals,
                    MissingStructuredPayloadSignals = structuredPayloadCoverage.MissingSignals,
                    StructuredPayloadSignalCoveredCount = structuredPayloadCoverage.CoveredSignalCount,
                    StructuredPayloadSignalMissingCount = structuredPayloadCoverage.MissingSignalCount,
                    StructuredPayloadSampleCount = structuredPayloadCoverage.SampleCount,
                    StructuredPayloadAvailable = structuredPayloadCoverage.StructuredDataAvailable,
                    StructuredPayloadSignalCoverageRatio = structuredPayloadCoverage.CoverageRatio,
                    StructuredPayloadSignalCoveragePercentagePoints = structuredPayloadCoverage.CoveragePercentagePoints,
                    StructuredPayloadSignalCoverageSummary = structuredPayloadCoverage.Summary,
                    RequiredStructuredResultSignals = dependencyProfile?.RequiredStructuredResultSignals ?? Array.Empty<string>(),
                    ObservedStructuredResultSignals = structuredResultCoverage.ObservedSignals,
                    MissingStructuredResultSignals = structuredResultCoverage.MissingSignals,
                    StructuredResultSignalCoveredCount = structuredResultCoverage.CoveredSignalCount,
                    StructuredResultSignalMissingCount = structuredResultCoverage.MissingSignalCount,
                    StructuredResultSampleCount = structuredResultCoverage.SampleCount,
                    StructuredResultAvailable = structuredResultCoverage.StructuredDataAvailable,
                    StructuredResultSignalCoverageRatio = structuredResultCoverage.CoverageRatio,
                    StructuredResultSignalCoveragePercentagePoints = structuredResultCoverage.CoveragePercentagePoints,
                    StructuredResultSignalCoverageSummary = structuredResultCoverage.Summary,
                    LegacyAlgorithmInputReadinessSummary = legacyAlgorithmInputReadinessSummary,
                    DependencyNotes = dependencyProfile?.Notes ?? string.Empty,
                    FormulaSignals = dependencyProfile?.FormulaSignals ?? Array.Empty<string>(),
                    CoveredFormulaSignalCount = formulaCoverage.CoveredCount,
                    MissingFormulaSignalCount = formulaCoverage.MissingCount,
                    CoveredFormulaSignals = formulaCoverage.CoveredItems,
                    MissingFormulaSignals = formulaCoverage.MissingItems,
                    FormulaSignalCoverageRatio = formulaCoverage.CoverageRatio,
                    FormulaSignalCoveragePercentagePoints = formulaCoverage.CoveragePercentagePoints,
                    FormulaSignalsBackedByObservedPayload = formulaEvidence.BackedByObservedPayload,
                    FormulaSignalsObservedPayloadFields = formulaEvidence.ObservedPayloadFields,
                    FormulaSignalObservedPayloadGaps = formulaEvidence.SignalOrRuleGaps
                        .Select(gap => new MotorYObservedAlgorithmEvidenceGapSnapshot
                        {
                            SignalOrRule = gap.SignalOrRule,
                            RequiredPayloadFields = gap.RequiredPayloadFields,
                            ObservedPayloadFields = gap.ObservedPayloadFields,
                            MissingPayloadFields = gap.MissingPayloadFields,
                            CoveredByObservedPayload = gap.CoveredByObservedPayload,
                            Summary = gap.Summary
                        })
                        .ToArray(),
                    FormulaSignalsObservedPayloadSummary = formulaEvidence.Summary,
                    LegacyAlgorithmRules = dependencyProfile?.LegacyAlgorithmRules ?? Array.Empty<string>(),
                    CoveredLegacyAlgorithmRuleCount = ruleCoverage.CoveredCount,
                    MissingLegacyAlgorithmRuleCount = ruleCoverage.MissingCount,
                    CoveredLegacyAlgorithmRules = ruleCoverage.CoveredItems,
                    MissingLegacyAlgorithmRules = ruleCoverage.MissingItems,
                    LegacyAlgorithmRuleCoverageRatio = ruleCoverage.CoverageRatio,
                    LegacyAlgorithmRuleCoveragePercentagePoints = ruleCoverage.CoveragePercentagePoints,
                    LegacyAlgorithmRulesBackedByObservedPayload = ruleEvidence.BackedByObservedPayload,
                    LegacyAlgorithmRulesObservedPayloadFields = ruleEvidence.ObservedPayloadFields,
                    LegacyAlgorithmRulesObservedPayloadGaps = ruleEvidence.SignalOrRuleGaps
                        .Select(gap => new MotorYObservedAlgorithmEvidenceGapSnapshot
                        {
                            SignalOrRule = gap.SignalOrRule,
                            RequiredPayloadFields = gap.RequiredPayloadFields,
                            ObservedPayloadFields = gap.ObservedPayloadFields,
                            MissingPayloadFields = gap.MissingPayloadFields,
                            CoveredByObservedPayload = gap.CoveredByObservedPayload,
                            Summary = gap.Summary
                        })
                        .ToArray(),
                    LegacyAlgorithmRulesObservedPayloadSummary = ruleEvidence.Summary,
                    FormulaSignalSummary = formulaCoverage.Summary,
                    LegacyAlgorithmRuleSummary = ruleCoverage.Summary,
                    SelectedMethodSummary = selection.SelectedMethodSummary,
                    BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
                    DependencyBuckets = dependencyBuckets,
                    Distributions = selection.Distributions
                };
            })
            .ToArray();
    }

    private static string BuildLegacyAlgorithmInputReadinessSummary(
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        bool legacyAlgorithmInputsReady)
    {
        var payloadStatus = payloadCoverage.RequiredPayloadFieldCoverageSummary;
        var ratedStatus = ratedCoverage.RequiredRatedParamFieldCoverageSummary;
        var resultStatus = resultCoverage.RequiredResultFieldCoverageSummary;
        var upstreamStatus = upstream.UpstreamDependencySummary;
        var rawDataStatus = rawDataCoverage.Summary;
        var structuredPayloadStatus = structuredPayloadCoverage.Summary;
        var structuredResultStatus = structuredResultCoverage.Summary;

        return legacyAlgorithmInputsReady
            ? $"legacy algorithm inputs ready; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {rawDataStatus}; {structuredPayloadStatus}; {structuredResultStatus}"
            : $"legacy algorithm inputs incomplete; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {rawDataStatus}; {structuredPayloadStatus}; {structuredResultStatus}";
    }

    private static IReadOnlyList<MotorYMethodRouteSelectionSnapshot> LoadMotorYMethodRouteSelections(SqliteConnection connection)
    {
        return LoadMotorYMethodDecisions(connection)
            .Select(MotorYMethodRouteSelectionSnapshotFactory.Create)
            .OrderBy(snapshot => snapshot.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static Dictionary<string, string> LoadMotorYSamplePayloadByCanonicalCode(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Method, COALESCE(Data, '{}')
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
ORDER BY rowid DESC;";

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode) || result.ContainsKey(canonicalCode))
            {
                continue;
            }

            var dataJson = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            if (!MotorYSamplePayloadCandidateValidator.IsValidBaselineCandidate(canonicalCode, dataJson))
            {
                continue;
            }

            result[canonicalCode] = dataJson;
        }

        return result;
    }

    private static Dictionary<string, MotorRatedParamsContract> LoadMotorYSampleRatedParamsByCanonicalCode(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT tri.Code, tri.Method, pt.Code, COALESCE(pt.RatedParams, '{}')
FROM TestRecordItems tri
JOIN TestRecords tr ON tr.Id = tri.TestRecordId
LEFT JOIN ProductTypes pt ON pt.Id = tr.TestProductTypeId
WHERE tri.Code IN (
    '直流电阻测定',
    '空载试验',
    '空载特性试验',
    '热试验',
    'A法负载试验',
    'B法负载试验',
    '堵转试验',
    '堵转特性试验')
ORDER BY tri.rowid DESC;";

        var result = new Dictionary<string, MotorRatedParamsContract>(StringComparer.Ordinal);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode) || result.ContainsKey(canonicalCode))
            {
                continue;
            }

            var productTypeCode = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var ratedParamsJson = reader.IsDBNull(3) ? "{}" : reader.GetString(3);
            var ratedParams = TryParseMotorRatedParams(productTypeCode, ratedParamsJson);
            if (ratedParams is null)
            {
                continue;
            }

            result[canonicalCode] = ratedParams;
        }

        return result;
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
            var distributions = ordered
                .Select(row => new MotorYMethodDistributionSnapshot
                {
                    MethodValue = row.Method,
                    Count = row.Count,
                    Share = totalCount <= 0
                        ? 0d
                        : Math.Round((double)row.Count / totalCount, 4, MidpointRounding.AwayFromZero),
                    Route = MotorYLegacyAlgorithmRouteResolver.Resolve(group.Key, row.Method)
                })
                .ToArray();

            result.Add(MotorYMethodDecisionFactory.Create(
                group.Key,
                totalCount,
                baselineRoute,
                baseline.Count,
                dominantRoute,
                dominant.Count,
                distributions,
                TestRecordViewMapper.MotorYDominantOverrideThreshold));
        }

        return result
            .OrderBy(snapshot => snapshot.CanonicalCode, StringComparer.Ordinal)
            .ToArray();
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
            var rawStandardCode = ReadStringish(root, "GB");

            return new MotorRatedParamsContract
            {
                ProductKind = "Motor_Y",
                Model = productTypeCode,
                StandardCode = NormalizeStandardCode(rawStandardCode),
                StandardCodeRaw = rawStandardCode,
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

    private static string NormalizeStandardCode(string rawStandardCode)
    {
        if (string.IsNullOrWhiteSpace(rawStandardCode))
        {
            return string.Empty;
        }

        return rawStandardCode switch
        {
            "0" => "GB1032_2012",
            "1" => "TB_朝阳电机",
            "2" => "GB1032_2023",
            _ => rawStandardCode
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
