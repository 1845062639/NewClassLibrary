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
                    samplePayloads.Keys,
                    upstreamObservedLegacyCodes);
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
                var intermediateResultCoverage = MotorYRequiredResultFieldCoverageEvaluator.Evaluate(
                    selection.CanonicalCode,
                    dependencyProfile?.RequiredIntermediateResultFields ?? Array.Empty<string>(),
                    sampleDataJson);
                var structuredPayloadCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
                    dependencyProfile?.RequiredStructuredPayloadSignals,
                    sampleDataJson,
                    "structured payload signals");
                var structuredResultCoverage = MotorYStructuredSignalCoverageEvaluator.Evaluate(
                    dependencyProfile?.RequiredStructuredResultSignals,
                    sampleDataJson,
                    "structured result signals");
                var observedStructuredSignals = structuredPayloadCoverage.ObservedSignals
                    .Concat(structuredResultCoverage.ObservedSignals)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var formulaEvidenceObservedFields = resultCoverage.CoveredRequiredResultFields
                    .Concat(rawDataSignalCoverage.ObservedSignals)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var formulaEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildFormulaSignalEvidence(
                    selection.CanonicalCode,
                    formulaEvidenceObservedFields,
                    observedStructuredSignals);
                var formulaCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
                    dependencyProfile?.FormulaSignals,
                    formulaEvidence.ObservedPayloadFields,
                    "formula signals");
                var ruleObservedFields = coverage.CoveredRequiredPayloadFields
                    .Concat(ratedCoverage.CoveredRequiredRatedParamFields)
                    .Concat(resultCoverage.CoveredRequiredResultFields)
                    .Concat(rawDataSignalCoverage.ObservedSignals)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                var ruleEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildLegacyRuleEvidence(
                    selection.CanonicalCode,
                    ruleObservedFields,
                    observedStructuredSignals);
                var ruleCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
                    dependencyProfile?.LegacyAlgorithmRules,
                    ruleEvidence.ObservedPayloadFields,
                    "legacy algorithm rules");
                var decisionAnchorObservedFields = ruleObservedFields;
                var decisionAnchorEvidence = MotorYObservedAlgorithmEvidenceCatalog.BuildLegacyDecisionAnchorEvidence(
                    selection.CanonicalCode,
                    decisionAnchorObservedFields,
                    observedStructuredSignals);
                var decisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
                    selection.CanonicalCode,
                    decisionAnchorObservedFields,
                    observedStructuredSignals);
                var decisionAnchorObservationGaps = decisionAnchorObservationRules
                    .Select(rule => new MotorYObservedAlgorithmEvidenceGap
                    {
                        SignalOrRule = $"decision-anchor-observation:{rule.AnchorKey}",
                        RequiredPayloadFields = rule.RequiredPayloadFields,
                        ObservedPayloadFields = rule.ObservedPayloadFields,
                        MissingPayloadFields = rule.MissingPayloadFields,
                        CoveredByObservedPayload = rule.CoveredByObservedPayload,
                        Summary = rule.Summary
                    })
                    .ToArray();
                var decisionAnchorCoverage = MotorYStructuredListCoverageEvaluator.Evaluate(
                    dependencyProfile?.LegacyDecisionAnchors,
                    decisionAnchorEvidence.ObservedPayloadFields,
                    "legacy decision anchors");
                var decisionAnchorResolutions = MotorYDecisionAnchorResolutionFactory.Build(selectedRoute.CanonicalCode, decisionAnchorObservationRules);
                var resolvedDecisionAnchorCount = decisionAnchorResolutions.Count(x => x.ResolvedByObservedPayload);
                var partialDecisionAnchorCount = decisionAnchorResolutions.Count(x => x.PartiallyResolvedByObservedPayload);
                var missingDecisionAnchorResolutionCount = decisionAnchorResolutions.Count - resolvedDecisionAnchorCount - partialDecisionAnchorCount;
                var decisionAnchorResolutionCoverageRatio = decisionAnchorResolutions.Count == 0
                    ? 1d
                    : Math.Round((double)resolvedDecisionAnchorCount / decisionAnchorResolutions.Count, 4, MidpointRounding.AwayFromZero);
                var decisionAnchorResolutionCoveragePercentagePoints = decisionAnchorResolutions.Count == 0
                    ? 100
                    : (int)Math.Round((double)resolvedDecisionAnchorCount / decisionAnchorResolutions.Count * 100d, MidpointRounding.AwayFromZero);
                var decisionAnchorResolutionSummary = MotorYDecisionAnchorResolutionFactory.BuildSummary(decisionAnchorResolutions);
                var decisionAnchorNextActionSummary = MotorYDecisionAnchorResolutionFactory.BuildNextActionSummary(decisionAnchorResolutions);
                var decisionAnchorGapPreviewSummary = MotorYDecisionAnchorResolutionFactory.BuildGapPreviewSummary(decisionAnchorResolutions);
                var decisionAnchorPriorityDistributions = MotorYDecisionAnchorResolutionFactory.BuildPriorityDistributions(decisionAnchorResolutions);
                var decisionAnchorPrioritySummary = MotorYDecisionAnchorResolutionFactory.BuildPrioritySummary(decisionAnchorResolutions);
                var topDecisionAnchorPriority = MotorYDecisionAnchorResolutionFactory.BuildTopPriorityDistribution(decisionAnchorResolutions);
                var suggestedDecisionAnchorNextSteps = MotorYDecisionAnchorResolutionFactory.BuildSuggestedNextSteps(decisionAnchorResolutions);
                var suggestedDecisionAnchorNextStepSummary = suggestedDecisionAnchorNextSteps.Count == 0
                    ? "no decision-anchor next-step recommendation"
                    : string.Join("; ", suggestedDecisionAnchorNextSteps);
                var legacyDecisionAnchorReady = missingDecisionAnchorResolutionCount == 0;
                var minimumRawSampleCount = dependencyProfile?.MinimumRawSampleCount ?? 0;
                var rawSampleCountReady = rawDataSignalCoverage.RawSampleCount >= minimumRawSampleCount;
                var rawSampleCountGap = Math.Max(0, minimumRawSampleCount - rawDataSignalCoverage.RawSampleCount);
                var rawSampleCountSummary = minimumRawSampleCount <= 0
                    ? $"raw sample count requirement not set; observed {rawDataSignalCoverage.RawSampleCount}"
                    : rawSampleCountReady
                        ? $"raw sample count ready {rawDataSignalCoverage.RawSampleCount}/{minimumRawSampleCount}"
                        : $"raw sample count insufficient {rawDataSignalCoverage.RawSampleCount}/{minimumRawSampleCount}";
                var rawSampleCountDecisionSummary = minimumRawSampleCount <= 0
                    ? $"raw sample count gate disabled for {selection.CanonicalCode}; observed {rawDataSignalCoverage.RawSampleCount}"
                    : rawSampleCountReady
                        ? $"raw sample count gate passed for {selection.CanonicalCode}: observed {rawDataSignalCoverage.RawSampleCount} >= required {minimumRawSampleCount}"
                        : $"raw sample count gate blocked for {selection.CanonicalCode}: observed {rawDataSignalCoverage.RawSampleCount}, still need {rawSampleCountGap} more samples to reach {minimumRawSampleCount}";
                var minimumStructuredPayloadSampleCount = dependencyProfile?.MinimumStructuredPayloadSampleCount ?? 0;
                var structuredPayloadSampleCountReady = structuredPayloadCoverage.SampleCount >= minimumStructuredPayloadSampleCount;
                var structuredPayloadSampleCountGap = Math.Max(0, minimumStructuredPayloadSampleCount - structuredPayloadCoverage.SampleCount);
                var structuredPayloadSampleCountSummary = minimumStructuredPayloadSampleCount <= 0
                    ? $"structured payload sample count requirement not set; observed {structuredPayloadCoverage.SampleCount}"
                    : structuredPayloadSampleCountReady
                        ? $"structured payload sample count ready {structuredPayloadCoverage.SampleCount}/{minimumStructuredPayloadSampleCount}"
                        : $"structured payload sample count insufficient {structuredPayloadCoverage.SampleCount}/{minimumStructuredPayloadSampleCount}";
                var structuredPayloadSampleCountDecisionSummary = minimumStructuredPayloadSampleCount <= 0
                    ? $"structured payload sample count gate disabled for {selection.CanonicalCode}; observed {structuredPayloadCoverage.SampleCount}"
                    : structuredPayloadSampleCountReady
                        ? $"structured payload sample count gate passed for {selection.CanonicalCode}: observed {structuredPayloadCoverage.SampleCount} >= required {minimumStructuredPayloadSampleCount}"
                        : $"structured payload sample count gate blocked for {selection.CanonicalCode}: observed {structuredPayloadCoverage.SampleCount}, still need {structuredPayloadSampleCountGap} more samples to reach {minimumStructuredPayloadSampleCount}";
                var minimumStructuredResultSampleCount = dependencyProfile?.MinimumStructuredResultSampleCount ?? 0;
                var structuredResultSampleCountReady = structuredResultCoverage.SampleCount >= minimumStructuredResultSampleCount;
                var structuredResultSampleCountGap = Math.Max(0, minimumStructuredResultSampleCount - structuredResultCoverage.SampleCount);
                var structuredResultSampleCountSummary = minimumStructuredResultSampleCount <= 0
                    ? $"structured result sample count requirement not set; observed {structuredResultCoverage.SampleCount}"
                    : structuredResultSampleCountReady
                        ? $"structured result sample count ready {structuredResultCoverage.SampleCount}/{minimumStructuredResultSampleCount}"
                        : $"structured result sample count insufficient {structuredResultCoverage.SampleCount}/{minimumStructuredResultSampleCount}";
                var structuredResultSampleCountDecisionSummary = minimumStructuredResultSampleCount <= 0
                    ? $"structured result sample count gate disabled for {selection.CanonicalCode}; observed {structuredResultCoverage.SampleCount}"
                    : structuredResultSampleCountReady
                        ? $"structured result sample count gate passed for {selection.CanonicalCode}: observed {structuredResultCoverage.SampleCount} >= required {minimumStructuredResultSampleCount}"
                        : $"structured result sample count gate blocked for {selection.CanonicalCode}: observed {structuredResultCoverage.SampleCount}, still need {structuredResultSampleCountGap} more samples to reach {minimumStructuredResultSampleCount}";
                var observedAlgorithmInputFields = coverage.CoveredRequiredPayloadFields
                    .Concat(ratedCoverage.CoveredRequiredRatedParamFields)
                    .Concat(resultCoverage.CoveredRequiredResultFields)
                    .Concat(intermediateResultCoverage.CoveredRequiredResultFields)
                    .Concat(rawDataSignalCoverage.ObservedSignals)
                    .Concat(structuredPayloadCoverage.ObservedSignals)
                    .Concat(structuredResultCoverage.ObservedSignals)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(field => field, StringComparer.Ordinal)
                    .ToArray();
                var missingAlgorithmInputFields = upstream.MissingUpstreamCanonicalCodes
                    .Concat(coverage.MissingRequiredPayloadFields)
                    .Concat(ratedCoverage.MissingRequiredRatedParamFields)
                    .Concat(resultCoverage.MissingRequiredResultFields)
                    .Concat(intermediateResultCoverage.MissingRequiredResultFields)
                    .Concat(rawDataSignalCoverage.MissingSignals)
                    .Concat(structuredPayloadCoverage.MissingSignals)
                    .Concat(structuredResultCoverage.MissingSignals)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(field => field, StringComparer.Ordinal)
                    .ToArray();
                var observedAlgorithmInputFieldSources = MotorYObservedFieldSourceCatalog.Build(
                    coverage.CoveredRequiredPayloadFields,
                    ratedCoverage.CoveredRequiredRatedParamFields,
                    resultCoverage.CoveredRequiredResultFields,
                    intermediateResultCoverage.CoveredRequiredResultFields,
                    rawDataSignalCoverage.ObservedSignals,
                    structuredPayloadCoverage.ObservedSignals,
                    structuredResultCoverage.ObservedSignals);
                var totalAlgorithmInputFieldCount = observedAlgorithmInputFields.Length + missingAlgorithmInputFields.Length;
                var algorithmInputFieldCoverageRatio = totalAlgorithmInputFieldCount == 0
                    ? 1d
                    : Math.Round((double)observedAlgorithmInputFields.Length / totalAlgorithmInputFieldCount, 4, MidpointRounding.AwayFromZero);
                var algorithmInputFieldCoveragePercentagePoints = (int)Math.Round(algorithmInputFieldCoverageRatio * 100d, MidpointRounding.AwayFromZero);
                var algorithmInputFieldCoverageSummary = $"algorithm input fields covered {observedAlgorithmInputFields.Length}/{totalAlgorithmInputFieldCount} ({algorithmInputFieldCoveragePercentagePoints}pp); missing: {(missingAlgorithmInputFields.Length == 0 ? "none" : string.Join(", ", missingAlgorithmInputFields))}; observed: {(observedAlgorithmInputFields.Length == 0 ? "none" : string.Join(", ", observedAlgorithmInputFields))}";
                var rawDataSignalsReady = rawDataSignalCoverage.MissingSignals.Count == 0;
                var structuredSignalsReady = structuredPayloadCoverage.MissingSignalCount == 0
                    && structuredResultCoverage.MissingSignalCount == 0;
                var requiredResultFieldsReady = resultCoverage.MissingRequiredResultFieldCount == 0;
                var requiredIntermediateResultFieldsReady = intermediateResultCoverage.MissingRequiredResultFieldCount == 0;
                var legacyAlgorithmInputsReady = upstream.UpstreamDependenciesSatisfied
                    && coverage.MissingRequiredPayloadFieldCount == 0
                    && ratedCoverage.MissingRequiredRatedParamFieldCount == 0
                    && requiredResultFieldsReady
                    && requiredIntermediateResultFieldsReady
                    && rawDataSignalsReady
                    && structuredSignalsReady
                    && rawSampleCountReady
                    && structuredPayloadSampleCountReady
                    && structuredResultSampleCountReady
                    && legacyDecisionAnchorReady;
                var legacyAlgorithmInputReadinessSummary = BuildLegacyAlgorithmInputReadinessSummary(
                    upstream,
                    coverage,
                    ratedCoverage,
                    resultCoverage,
                    intermediateResultCoverage,
                    rawDataSignalCoverage,
                    structuredPayloadCoverage,
                    structuredResultCoverage,
                    rawSampleCountReady,
                    rawSampleCountSummary,
                    structuredPayloadSampleCountReady,
                    structuredPayloadSampleCountSummary,
                    structuredResultSampleCountReady,
                    structuredResultSampleCountSummary,
                    legacyDecisionAnchorReady,
                    decisionAnchorResolutionSummary,
                    legacyAlgorithmInputsReady);
                var suggestedNextSteps = BuildSuggestedNextSteps(
                    selection.CanonicalCode,
                    upstream,
                    coverage,
                    ratedCoverage,
                    resultCoverage,
                    intermediateResultCoverage,
                    rawDataSignalCoverage,
                    structuredPayloadCoverage,
                    structuredResultCoverage,
                    decisionAnchorResolutions);
                var suggestedNextStepSummary = suggestedNextSteps.Count == 0
                    ? "no immediate next-step recommendation"
                    : string.Join("; ", suggestedNextSteps);
                var dependencyBuckets = MotorYDependencyBucketSummaryFactory.Create(
                    upstream,
                    coverage,
                    ratedCoverage,
                    resultCoverage,
                    intermediateResultCoverage,
                    rawDataSignalCoverage,
                    structuredPayloadCoverage,
                    structuredResultCoverage,
                    formulaCoverage,
                    ruleCoverage,
                    decisionAnchorCoverage,
                    decisionAnchorResolutions);

                return new MotorYMethodAdaptationPlanSnapshot
                {
                    CanonicalCode = selection.CanonicalCode,
                    DecisionAnchorTopPriority = topDecisionAnchorPriority?.Priority ?? string.Empty,
                    DecisionAnchorTopPrioritySummary = topDecisionAnchorPriority is null
                        ? "decision anchor top priority unavailable"
                        : $"top decision anchor priority={topDecisionAnchorPriority.Priority}; focus={topDecisionAnchorPriority.DominantSuggestedNextStepFocus}; anchor={topDecisionAnchorPriority.DominantAnchorKey}; fields={(topDecisionAnchorPriority.DominantSuggestedNextStepFields.Count == 0 ? "none" : string.Join(", ", topDecisionAnchorPriority.DominantSuggestedNextStepFields))}",
                    DecisionAnchorTopPriorityDominantAnchorKey = topDecisionAnchorPriority?.DominantAnchorKey ?? string.Empty,
                    DecisionAnchorTopPriorityFocus = topDecisionAnchorPriority?.DominantSuggestedNextStepFocus ?? string.Empty,
                    DecisionAnchorTopPriorityFields = topDecisionAnchorPriority?.DominantSuggestedNextStepFields ?? Array.Empty<string>(),
                    DecisionAnchorTopPriorityNextStepSummary = topDecisionAnchorPriority?.DominantSuggestedNextStepSummary ?? string.Empty,
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
                    ObservedUpstreamLegacyCodes = upstream.ObservedUpstreamLegacyCodes,
                    MissingUpstreamCanonicalCodes = upstream.MissingUpstreamCanonicalCodes,
                    UpstreamDependenciesSatisfied = upstream.UpstreamDependenciesSatisfied,
                    UpstreamDependencySummary = upstream.UpstreamDependencySummary,
                    RequiredPayloadFields = requiredPayloadFields,
                    RequiredRatedParamFields = dependencyProfile?.RequiredRatedParamFields ?? Array.Empty<string>(),
                    RequiredResultFields = dependencyProfile?.RequiredResultFields ?? Array.Empty<string>(),
                    RequiredIntermediateResultFields = dependencyProfile?.RequiredIntermediateResultFields ?? Array.Empty<string>(),
                    CoveredRequiredIntermediateResultFieldCount = intermediateResultCoverage.CoveredRequiredResultFieldCount,
                    MissingRequiredIntermediateResultFieldCount = intermediateResultCoverage.MissingRequiredResultFieldCount,
                    MissingRequiredIntermediateResultFields = intermediateResultCoverage.MissingRequiredResultFields,
                    CoveredRequiredIntermediateResultFields = intermediateResultCoverage.CoveredRequiredResultFields,
                    RequiredIntermediateResultFieldCoverageRatio = intermediateResultCoverage.RequiredResultFieldCoverageRatio,
                    RequiredIntermediateResultFieldCoveragePercentagePoints = intermediateResultCoverage.RequiredResultFieldCoveragePercentagePoints,
                    RequiredIntermediateResultFieldCoverageSummary = intermediateResultCoverage.RequiredResultFieldCoverageSummary,
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
                    ObservedAlgorithmInputFields = observedAlgorithmInputFields,
                    ObservedAlgorithmInputFieldSources = observedAlgorithmInputFieldSources,
                    MissingAlgorithmInputFields = missingAlgorithmInputFields,
                    ObservedAlgorithmInputFieldCount = observedAlgorithmInputFields.Length,
                    MissingAlgorithmInputFieldCount = missingAlgorithmInputFields.Length,
                    AlgorithmInputFieldCoverageRatio = algorithmInputFieldCoverageRatio,
                    AlgorithmInputFieldCoveragePercentagePoints = algorithmInputFieldCoveragePercentagePoints,
                    AlgorithmInputFieldCoverageSummary = algorithmInputFieldCoverageSummary,
                    RawDataSignalsReady = rawDataSignalsReady,
                    MinimumRawSampleCount = minimumRawSampleCount,
                    RawSampleCountReady = rawSampleCountReady,
                    RawSampleCountReadinessSummary = rawSampleCountSummary,
                    RawSampleCountGap = rawSampleCountGap,
                    RawSampleCountDecisionSummary = rawSampleCountDecisionSummary,
                    RequiredStructuredPayloadSignals = dependencyProfile?.RequiredStructuredPayloadSignals ?? Array.Empty<string>(),
                    MinimumStructuredPayloadSampleCount = minimumStructuredPayloadSampleCount,
                    StructuredPayloadSampleCountReady = structuredPayloadSampleCountReady,
                    StructuredPayloadSampleCountReadinessSummary = structuredPayloadSampleCountSummary,
                    StructuredPayloadSampleCountGap = structuredPayloadSampleCountGap,
                    StructuredPayloadSampleCountDecisionSummary = structuredPayloadSampleCountDecisionSummary,
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
                    MinimumStructuredResultSampleCount = minimumStructuredResultSampleCount,
                    StructuredResultSampleCountReady = structuredResultSampleCountReady,
                    StructuredResultSampleCountReadinessSummary = structuredResultSampleCountSummary,
                    StructuredResultSampleCountGap = structuredResultSampleCountGap,
                    StructuredResultSampleCountDecisionSummary = structuredResultSampleCountDecisionSummary,
                    LegacyAlgorithmInputReadinessSummary = legacyAlgorithmInputReadinessSummary,
                    DependencyNotes = dependencyProfile?.Notes ?? string.Empty,
                    SuggestedNextSteps = suggestedNextSteps,
                    SuggestedNextStepSummary = suggestedNextStepSummary,
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
                    LegacyDecisionAnchors = dependencyProfile?.LegacyDecisionAnchors ?? Array.Empty<string>(),
                    CoveredLegacyDecisionAnchorCount = decisionAnchorCoverage.CoveredCount,
                    MissingLegacyDecisionAnchorCount = decisionAnchorCoverage.MissingCount,
                    CoveredLegacyDecisionAnchors = decisionAnchorCoverage.CoveredItems,
                    MissingLegacyDecisionAnchors = decisionAnchorCoverage.MissingItems,
                    LegacyDecisionAnchorCoverageRatio = decisionAnchorCoverage.CoverageRatio,
                    LegacyDecisionAnchorCoveragePercentagePoints = decisionAnchorCoverage.CoveragePercentagePoints,
                    LegacyDecisionAnchorsBackedByObservedPayload = decisionAnchorEvidence.BackedByObservedPayload,
                    LegacyDecisionAnchorReady = legacyDecisionAnchorReady,
                    LegacyDecisionAnchorsObservedPayloadFields = decisionAnchorEvidence.ObservedPayloadFields,
                    LegacyDecisionAnchorsObservedPayloadGaps = decisionAnchorObservationGaps
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
                    LegacyDecisionAnchorObservationRules = decisionAnchorObservationRules
                        .Select(rule => new MotorYDecisionAnchorObservationRuleSnapshot
                        {
                            AnchorKey = rule.AnchorKey,
                            RequiredPayloadFields = rule.RequiredPayloadFields,
                            ObservedPayloadFields = rule.ObservedPayloadFields,
                            MissingPayloadFields = rule.MissingPayloadFields,
                            CoveredByObservedPayload = rule.CoveredByObservedPayload,
                            Summary = rule.Summary
                        })
                        .ToArray(),
                    CoveredLegacyDecisionAnchorObservationRuleCount = decisionAnchorObservationRules.Count(rule => rule.CoveredByObservedPayload),
                    MissingLegacyDecisionAnchorObservationRuleCount = decisionAnchorObservationRules.Count(rule => !rule.CoveredByObservedPayload),
                    LegacyDecisionAnchorObservationRuleCoverageRatio = decisionAnchorObservationRules.Count == 0
                        ? 1d
                        : Math.Round((double)decisionAnchorObservationRules.Count(rule => rule.CoveredByObservedPayload) / decisionAnchorObservationRules.Count, 4, MidpointRounding.AwayFromZero),
                    LegacyDecisionAnchorObservationRuleCoveragePercentagePoints = decisionAnchorObservationRules.Count == 0
                        ? 100
                        : (int)Math.Round((double)decisionAnchorObservationRules.Count(rule => rule.CoveredByObservedPayload) / decisionAnchorObservationRules.Count * 100d, MidpointRounding.AwayFromZero),
                    LegacyDecisionAnchorObservationRuleSummary = BuildDecisionAnchorObservationRuleSummary(decisionAnchorObservationRules),
                    ResolvedLegacyDecisionAnchorCount = decisionAnchorResolutions.Count(resolution => resolution.ResolvedByObservedPayload),
                    PartialLegacyDecisionAnchorCount = decisionAnchorResolutions.Count(resolution => resolution.PartiallyResolvedByObservedPayload),
                    MissingLegacyDecisionAnchorResolutionCount = decisionAnchorResolutions.Count(resolution => !resolution.ResolvedByObservedPayload && !resolution.PartiallyResolvedByObservedPayload),
                    EffectiveLegacyDecisionAnchorCoverageCount = decisionAnchorResolutions.Count(resolution => resolution.ResolvedByObservedPayload || resolution.PartiallyResolvedByObservedPayload),
                    EffectiveLegacyDecisionAnchorGapCount = decisionAnchorResolutions.Count(resolution => !resolution.ResolvedByObservedPayload && !resolution.PartiallyResolvedByObservedPayload),
                    EffectiveLegacyDecisionAnchorCoverageRatio = decisionAnchorResolutions.Count == 0
                        ? 1d
                        : Math.Round((double)decisionAnchorResolutions.Count(resolution => resolution.ResolvedByObservedPayload || resolution.PartiallyResolvedByObservedPayload) / decisionAnchorResolutions.Count, 4, MidpointRounding.AwayFromZero),
                    EffectiveLegacyDecisionAnchorCoveragePercentagePoints = decisionAnchorResolutions.Count == 0
                        ? 100
                        : (int)Math.Round((double)decisionAnchorResolutions.Count(resolution => resolution.ResolvedByObservedPayload || resolution.PartiallyResolvedByObservedPayload) / decisionAnchorResolutions.Count * 100d, MidpointRounding.AwayFromZero),
                    LegacyDecisionAnchorResolutions = decisionAnchorResolutions
                        .Select(resolution => new MotorYDecisionAnchorResolutionSnapshot
                        {
                            AnchorKey = resolution.AnchorKey,
                            ResolvedByObservedPayload = resolution.ResolvedByObservedPayload,
                            PartiallyResolvedByObservedPayload = resolution.PartiallyResolvedByObservedPayload,
                            RequiredPayloadFields = resolution.RequiredPayloadFields,
                            ObservedPayloadFields = resolution.ObservedPayloadFields,
                            MissingPayloadFields = resolution.MissingPayloadFields,
                            CoverageRatio = resolution.CoverageRatio,
                            CoveragePercentagePoints = resolution.CoveragePercentagePoints,
                            ResolutionStage = resolution.ResolutionStage,
                            SuggestedNextStepCategory = resolution.SuggestedNextStepCategory,
                            SuggestedNextStepFocus = resolution.SuggestedNextStepFocus,
                            SuggestedNextStepFields = resolution.SuggestedNextStepFields,
                            SuggestedNextSteps = resolution.SuggestedNextSteps,
                            SuggestedNextStepSummary = resolution.SuggestedNextStepSummary,
                            SuggestedNextStepPriority = resolution.SuggestedNextStepPriority,
                            SuggestedNextStepPrioritySummary = resolution.SuggestedNextStepPrioritySummary,
                            SuggestedNextStepCoverageSummary = resolution.SuggestedNextStepCoverageSummary,
                            Summary = resolution.Summary
                        })
                        .ToArray(),
                    LegacyDecisionAnchorResolutionSummary = decisionAnchorResolutionSummary,
                    LegacyDecisionAnchorNextActionSummary = decisionAnchorNextActionSummary,
                    LegacyDecisionAnchorGapPreviewSummary = decisionAnchorGapPreviewSummary,
                    DecisionAnchorPriorityDistributions = decisionAnchorPriorityDistributions
                        .Select(distribution => new MotorYDecisionAnchorPriorityDistributionSnapshot
                        {
                            Priority = distribution.Priority,
                            Count = distribution.Count,
                            Share = distribution.Share,
                            AnchorKeys = distribution.AnchorKeys,
                            SuggestedNextStepFocuses = distribution.SuggestedNextStepFocuses,
                            SuggestedNextStepFields = distribution.SuggestedNextStepFields,
                            SuggestedNextSteps = distribution.SuggestedNextSteps,
                            SuggestedNextStepSummary = distribution.SuggestedNextStepSummary,
                            DominantAnchorKey = distribution.DominantAnchorKey,
                            DominantSuggestedNextStepFocus = distribution.DominantSuggestedNextStepFocus,
                            DominantSuggestedNextStepFields = distribution.DominantSuggestedNextStepFields,
                            DominantSuggestedNextStepSummary = distribution.DominantSuggestedNextStepSummary
                        })
                        .ToArray(),
                    DecisionAnchorPrioritySummary = decisionAnchorPrioritySummary,
                    SuggestedDecisionAnchorNextSteps = suggestedDecisionAnchorNextSteps,
                    SuggestedDecisionAnchorNextStepSummary = suggestedDecisionAnchorNextStepSummary,
                    LegacyDecisionAnchorsObservedPayloadSummary = decisionAnchorEvidence.Summary,
                    FormulaSignalSummary = formulaCoverage.Summary,
                    LegacyAlgorithmRuleSummary = ruleCoverage.Summary,
                    LegacyDecisionAnchorSummary = decisionAnchorCoverage.Summary,
                    SelectedMethodSummary = selection.SelectedMethodSummary,
                    BaselineDominantComparisonSummary = selection.BaselineDominantComparisonSummary,
                    DependencyBuckets = dependencyBuckets,
                    Distributions = selection.Distributions
                };
            })
            .ToArray();
    }

    private static IReadOnlyList<string> BuildSuggestedNextSteps(
        string canonicalCode,
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        IReadOnlyList<MotorYDecisionAnchorResolution> decisionAnchorResolutions)
    {
        var steps = new List<string>();

        if (upstream.MissingUpstreamCanonicalCodes.Count > 0)
        {
            steps.Add($"补齐上游试验项: {string.Join(", ", upstream.MissingUpstreamCanonicalCodes)}");
        }

        if (payloadCoverage.MissingRequiredPayloadFields.Count > 0)
        {
            steps.Add($"补齐 payload 字段: {FormatPreview(payloadCoverage.MissingRequiredPayloadFields, 4)}");
        }

        if (ratedCoverage.MissingRequiredRatedParamFields.Count > 0)
        {
            steps.Add($"补齐额定参数字段: {FormatPreview(ratedCoverage.MissingRequiredRatedParamFields, 4)}");
        }

        if (intermediateResultCoverage.MissingRequiredResultFields.Count > 0)
        {
            steps.Add($"优先回填中间结果字段: {FormatPreview(intermediateResultCoverage.MissingRequiredResultFields, 4)}");
        }

        if (resultCoverage.MissingRequiredResultFields.Count > 0)
        {
            steps.Add($"补齐结果字段: {FormatPreview(resultCoverage.MissingRequiredResultFields, 4)}");
        }

        if (rawDataCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐原始采样信号: {FormatPreview(rawDataCoverage.MissingSignals, 4)}");
        }

        if (structuredPayloadCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐结构化 payload 信号: {FormatPreview(structuredPayloadCoverage.MissingSignals, 4)}");
        }

        if (structuredResultCoverage.MissingSignals.Count > 0)
        {
            steps.Add($"补齐结构化结果信号: {FormatPreview(structuredResultCoverage.MissingSignals, 4)}");
        }

        var unresolvedAnchors = decisionAnchorResolutions
            .Where(x => !x.ResolvedByObservedPayload)
            .Select(x => x.AnchorKey)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (unresolvedAnchors.Length > 0)
        {
            steps.Add($"补齐决策锚点观测依据: {FormatPreview(unresolvedAnchors, 3)}");
        }

        if (steps.Count == 0)
        {
            steps.Add($"{canonicalCode} 已具备旧算法适配输入，可进入 adapter 迁移/结果校对");
        }

        return steps.Take(4).ToArray();
    }

    private static string FormatPreview(IEnumerable<string> values, int maxCount)
    {
        var items = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Take(maxCount + 1)
            .ToArray();

        if (items.Length == 0)
        {
            return "none";
        }

        if (items.Length <= maxCount)
        {
            return string.Join(", ", items);
        }

        return string.Join(", ", items.Take(maxCount)) + ", ...";
    }

    private static string BuildDecisionAnchorObservationRuleSummary(IReadOnlyList<MotorYDecisionAnchorObservationRule> rules)
    {
        if (rules.Count == 0)
        {
            return "decision anchor observation rules unavailable";
        }

        var covered = rules.Count(rule => rule.CoveredByObservedPayload);
        var missing = rules.Count - covered;
        var ratio = Math.Round((double)covered / rules.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);
        var missingAnchorKeys = rules
            .Where(rule => !rule.CoveredByObservedPayload)
            .Select(rule => rule.AnchorKey)
            .ToArray();

        return $"decision anchor observation rules covered {covered}/{rules.Count} ({percentagePoints}pp); missing: {(missing == 0 ? "none" : string.Join(", ", missingAnchorKeys))}";
    }

    private static string BuildLegacyAlgorithmInputReadinessSummary(
        MotorYUpstreamDependencySnapshot upstream,
        MotorYRequiredPayloadFieldCoverageSnapshot payloadCoverage,
        MotorYRequiredRatedParamFieldCoverageSnapshot ratedCoverage,
        MotorYRequiredResultFieldCoverageSnapshot resultCoverage,
        MotorYRequiredResultFieldCoverageSnapshot intermediateResultCoverage,
        MotorYRawDataSignalCoverageSnapshot rawDataCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredPayloadCoverage,
        MotorYStructuredSignalCoverageSnapshot structuredResultCoverage,
        bool rawSampleCountReady,
        string rawSampleCountSummary,
        bool structuredPayloadSampleCountReady,
        string structuredPayloadSampleCountSummary,
        bool structuredResultSampleCountReady,
        string structuredResultSampleCountSummary,
        bool legacyDecisionAnchorReady,
        string decisionAnchorResolutionSummary,
        bool legacyAlgorithmInputsReady)
    {
        var payloadStatus = payloadCoverage.RequiredPayloadFieldCoverageSummary;
        var ratedStatus = ratedCoverage.RequiredRatedParamFieldCoverageSummary;
        var resultStatus = resultCoverage.RequiredResultFieldCoverageSummary;
        var intermediateResultStatus = intermediateResultCoverage.RequiredResultFieldCoverageSummary;
        var upstreamStatus = upstream.UpstreamDependencySummary;
        var rawDataStatus = rawDataCoverage.Summary;
        var structuredPayloadStatus = structuredPayloadCoverage.Summary;
        var structuredResultStatus = structuredResultCoverage.Summary;
        var decisionAnchorStatus = legacyDecisionAnchorReady
            ? $"decision anchor ready; {decisionAnchorResolutionSummary}"
            : $"decision anchor incomplete; {decisionAnchorResolutionSummary}";
        var rawSampleStatus = rawSampleCountReady
            ? rawSampleCountSummary
            : rawSampleCountSummary;
        var structuredPayloadSampleStatus = structuredPayloadSampleCountReady
            ? structuredPayloadSampleCountSummary
            : structuredPayloadSampleCountSummary;
        var structuredResultSampleStatus = structuredResultSampleCountReady
            ? structuredResultSampleCountSummary
            : structuredResultSampleCountSummary;

        return legacyAlgorithmInputsReady
            ? $"legacy algorithm inputs ready; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {intermediateResultStatus}; {rawDataStatus}; {rawSampleStatus}; {structuredPayloadStatus}; {structuredPayloadSampleStatus}; {structuredResultStatus}; {structuredResultSampleStatus}; {decisionAnchorStatus}"
            : $"legacy algorithm inputs incomplete; {upstreamStatus}; {payloadStatus}; {ratedStatus}; {resultStatus}; {intermediateResultStatus}; {rawDataStatus}; {rawSampleStatus}; {structuredPayloadStatus}; {structuredPayloadSampleStatus}; {structuredResultStatus}; {structuredResultSampleStatus}; {decisionAnchorStatus}";
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

        var decisionSelections = LoadMotorYMethodDecisions(connection)
            .Select(MotorYMethodRouteSelectionSnapshotFactory.Create)
            .Where(selection => selection.SelectedRoute is not null)
            .ToDictionary(
                selection => selection.CanonicalCode,
                selection => selection.SelectedRoute!.MethodValue,
                StringComparer.Ordinal);
        var fallbackCandidates = new Dictionary<string, string>(StringComparer.Ordinal);
        var preferredCandidates = new Dictionary<string, string>(StringComparer.Ordinal);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            var canonicalCode = MotorYLegacyItemCodeNormalizer.Normalize(legacyCode);
            if (!MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(canonicalCode))
            {
                continue;
            }

            var methodValue = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            var dataJson = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            if (!MotorYSamplePayloadCandidateValidator.IsValidBaselineCandidate(canonicalCode, dataJson))
            {
                continue;
            }

            if (!fallbackCandidates.ContainsKey(canonicalCode))
            {
                fallbackCandidates[canonicalCode] = dataJson;
            }

            if (preferredCandidates.ContainsKey(canonicalCode))
            {
                continue;
            }

            if (decisionSelections.TryGetValue(canonicalCode, out var selectedMethod)
                && methodValue == selectedMethod
                && MotorYSamplePayloadCandidateValidator.IsPreferredMethodCandidate(canonicalCode, methodValue, dataJson))
            {
                preferredCandidates[canonicalCode] = dataJson;
            }
        }

        return fallbackCandidates
            .ToDictionary(
                pair => pair.Key,
                pair => preferredCandidates.TryGetValue(pair.Key, out var preferred) ? preferred : pair.Value,
                StringComparer.Ordinal);
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
