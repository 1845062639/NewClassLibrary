using Microsoft.Data.Sqlite;

namespace StandardTestNext.Test.Application.Services;

public sealed class StpDbSnapshotQueryService
{
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
    tr.IsValid
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
                IsValid = !reader.IsDBNull(11) && reader.GetInt64(11) == 1
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
SELECT ID, COALESCE(Code, ''), COALESCE(RatedParams, '{{}}'), Category, Manufacturer, Remark, IsValid, CreateTime, UpdateTime
FROM ProductTypes
WHERE ID IN ({string.Join(", ", parameterNames)});";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var snapshot = new StpDbProductTypeSnapshot
            {
                Id = reader.GetString(0),
                Code = reader.GetString(1),
                RatedParamsJson = reader.GetString(2),
                Category = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Manufacturer = reader.IsDBNull(4) ? null : reader.GetString(4),
                Remark = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsValid = !reader.IsDBNull(6) && reader.GetInt64(6) == 1,
                CreateTimeRaw = reader.IsDBNull(7) ? null : reader.GetString(7),
                UpdateTimeRaw = reader.IsDBNull(8) ? null : reader.GetString(8)
            };

            result[snapshot.Id] = snapshot;
        }

        return result;
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
SELECT ID, COALESCE(Code, ''), Method, COALESCE(Data, '{{}}'), Remark, TestRecordId, IsValid
FROM TestRecordItems
WHERE TestRecordId IN ({string.Join(", ", parameterNames)})
  AND Code IN ({BuildInlineQuotedList(MotorYLegacyItemCodes)});";

        var items = new List<StpDbTestRecordItemSnapshot>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var id = reader.GetString(0);
            items.Add(new StpDbTestRecordItemSnapshot
            {
                Id = id,
                Code = reader.GetString(1),
                Method = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                DataJson = reader.GetString(3),
                Remark = reader.IsDBNull(4) ? null : reader.GetString(4),
                TestRecordId = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsValid = !reader.IsDBNull(6) && reader.GetInt64(6) == 1,
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

    private static string BuildInlineQuotedList(IEnumerable<string> values)
    {
        return string.Join(", ", values.Select(static value => $"'{value.Replace("'", "''")}'"));
    }
}
