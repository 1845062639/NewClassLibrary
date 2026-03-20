using Microsoft.Data.Sqlite;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Domain.Records;
using System.Text.Json;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public static class SQLiteTestPersistence
{
    public static string DefaultDbPath => Path.Combine(AppContext.BaseDirectory, "artifacts", "test-persistence", "standardtest-next.db");

    public static void EnsureCreated(string? dbPath = null)
    {
        var path = dbPath ?? DefaultDbPath;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS ProductDefinitions (
    ProductId TEXT PRIMARY KEY,
    ProductKind TEXT NOT NULL,
    Code TEXT NOT NULL,
    Model TEXT NOT NULL,
    Manufacturer TEXT NOT NULL,
    RatedParamsJson TEXT NOT NULL,
    Remark TEXT NULL,
    IsValid INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS TestRecords (
    TestRecordId TEXT PRIMARY KEY,
    RecordCode TEXT NOT NULL UNIQUE,
    SerialNumber TEXT NOT NULL,
    ProductKind TEXT NOT NULL,
    TestKindCode TEXT NOT NULL,
    OwnDepartment TEXT NULL,
    TestDepartment TEXT NULL,
    Tester TEXT NULL,
    Remark TEXT NULL,
    TestTime TEXT NOT NULL,
    IsValid INTEGER NOT NULL,
    TestProductId TEXT NULL,
    AccompanyProductId TEXT NULL,
    AggregateJson TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TestRecordAttachments (
    AttachmentId TEXT PRIMARY KEY,
    OwnerType TEXT NOT NULL,
    OwnerId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    FileExt TEXT NOT NULL,
    StoragePath TEXT NOT NULL,
    Length INTEGER NOT NULL,
    UploadedAt TEXT NOT NULL,
    ExtraInfoJson TEXT NULL
);

CREATE TABLE IF NOT EXISTS TestReports (
    RecordCode TEXT NOT NULL,
    Format TEXT NOT NULL,
    SavedAt TEXT NOT NULL,
    Content TEXT NOT NULL,
    PRIMARY KEY (RecordCode, Format)
);

CREATE TABLE IF NOT EXISTS TestReportSummaries (
    RecordCode TEXT NOT NULL,
    Format TEXT NOT NULL,
    ArtifactFileName TEXT NOT NULL,
    ArtifactSavedPath TEXT NOT NULL,
    ExportedAt TEXT NOT NULL,
    ContentLength INTEGER NOT NULL,
    PRIMARY KEY (RecordCode, Format)
);
";
        command.ExecuteNonQuery();
    }
}

public sealed class SQLiteProductDefinitionRepository : IProductDefinitionRepository
{
    private readonly string _dbPath;

    public SQLiteProductDefinitionRepository(string? dbPath = null)
    {
        _dbPath = dbPath ?? SQLiteTestPersistence.DefaultDbPath;
        SQLiteTestPersistence.EnsureCreated(_dbPath);
    }

    public async Task<ProductDefinition?> FindByKindAsync(string productKind, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT ProductId, ProductKind, Code, Model, Manufacturer, RatedParamsJson, Remark, IsValid
FROM ProductDefinitions
WHERE ProductKind = $productKind
LIMIT 1;";
        command.Parameters.AddWithValue("$productKind", productKind);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ProductDefinition
        {
            ProductId = Guid.Parse(reader.GetString(0)),
            ProductKind = reader.GetString(1),
            Code = reader.GetString(2),
            Model = reader.GetString(3),
            Manufacturer = reader.GetString(4),
            RatedParamsJson = reader.GetString(5),
            Remark = reader.IsDBNull(6) ? null : reader.GetString(6),
            IsValid = reader.GetInt64(7) == 1
        };
    }

    public async Task<IReadOnlyList<ProductDefinition>> ListRecentAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT ProductId, ProductKind, Code, Model, Manufacturer, RatedParamsJson, Remark, IsValid
FROM ProductDefinitions
ORDER BY rowid DESC
LIMIT $take;";
        command.Parameters.AddWithValue("$take", Math.Max(1, take));

        var items = new List<ProductDefinition>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductDefinition
            {
                ProductId = Guid.Parse(reader.GetString(0)),
                ProductKind = reader.GetString(1),
                Code = reader.GetString(2),
                Model = reader.GetString(3),
                Manufacturer = reader.GetString(4),
                RatedParamsJson = reader.GetString(5),
                Remark = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsValid = reader.GetInt64(7) == 1
            });
        }

        return items;
    }

    public async Task SaveAsync(ProductDefinition product, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO ProductDefinitions (ProductId, ProductKind, Code, Model, Manufacturer, RatedParamsJson, Remark, IsValid)
VALUES ($productId, $productKind, $code, $model, $manufacturer, $ratedParamsJson, $remark, $isValid)
ON CONFLICT(ProductId) DO UPDATE SET
    ProductKind = excluded.ProductKind,
    Code = excluded.Code,
    Model = excluded.Model,
    Manufacturer = excluded.Manufacturer,
    RatedParamsJson = excluded.RatedParamsJson,
    Remark = excluded.Remark,
    IsValid = excluded.IsValid;";
        command.Parameters.AddWithValue("$productId", product.ProductId.ToString());
        command.Parameters.AddWithValue("$productKind", product.ProductKind);
        command.Parameters.AddWithValue("$code", product.Code);
        command.Parameters.AddWithValue("$model", product.Model);
        command.Parameters.AddWithValue("$manufacturer", product.Manufacturer);
        command.Parameters.AddWithValue("$ratedParamsJson", product.RatedParamsJson);
        command.Parameters.AddWithValue("$remark", (object?)product.Remark ?? DBNull.Value);
        command.Parameters.AddWithValue("$isValid", product.IsValid ? 1 : 0);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class SQLiteTestRecordRepository : ITestRecordRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = false };

    public SQLiteTestRecordRepository(string? dbPath = null)
    {
        _dbPath = dbPath ?? SQLiteTestPersistence.DefaultDbPath;
        SQLiteTestPersistence.EnsureCreated(_dbPath);
    }

    public async Task SaveAsync(TestRecordAggregate record, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO TestRecords (
    TestRecordId, RecordCode, SerialNumber, ProductKind, TestKindCode,
    OwnDepartment, TestDepartment, Tester, Remark, TestTime, IsValid,
    TestProductId, AccompanyProductId, AggregateJson)
VALUES (
    $testRecordId, $recordCode, $serialNumber, $productKind, $testKindCode,
    $ownDepartment, $testDepartment, $tester, $remark, $testTime, $isValid,
    $testProductId, $accompanyProductId, $aggregateJson)
ON CONFLICT(TestRecordId) DO UPDATE SET
    RecordCode = excluded.RecordCode,
    SerialNumber = excluded.SerialNumber,
    ProductKind = excluded.ProductKind,
    TestKindCode = excluded.TestKindCode,
    OwnDepartment = excluded.OwnDepartment,
    TestDepartment = excluded.TestDepartment,
    Tester = excluded.Tester,
    Remark = excluded.Remark,
    TestTime = excluded.TestTime,
    IsValid = excluded.IsValid,
    TestProductId = excluded.TestProductId,
    AccompanyProductId = excluded.AccompanyProductId,
    AggregateJson = excluded.AggregateJson;";
        command.Parameters.AddWithValue("$testRecordId", record.TestRecordId.ToString());
        command.Parameters.AddWithValue("$recordCode", record.RecordCode);
        command.Parameters.AddWithValue("$serialNumber", record.SerialNumber);
        command.Parameters.AddWithValue("$productKind", record.ProductKind);
        command.Parameters.AddWithValue("$testKindCode", record.TestKindCode);
        command.Parameters.AddWithValue("$ownDepartment", (object?)record.OwnDepartment ?? DBNull.Value);
        command.Parameters.AddWithValue("$testDepartment", (object?)record.TestDepartment ?? DBNull.Value);
        command.Parameters.AddWithValue("$tester", (object?)record.Tester ?? DBNull.Value);
        command.Parameters.AddWithValue("$remark", (object?)record.Remark ?? DBNull.Value);
        command.Parameters.AddWithValue("$testTime", record.TestTime.ToString("O"));
        command.Parameters.AddWithValue("$isValid", record.IsValid ? 1 : 0);
        command.Parameters.AddWithValue("$testProductId", record.TestProduct?.ProductId.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$accompanyProductId", record.AccompanyProduct?.ProductId.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$aggregateJson", JsonSerializer.Serialize(record, _jsonOptions));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TestRecordAggregate?> FindByRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT AggregateJson
FROM TestRecords
WHERE RecordCode = $recordCode
LIMIT 1;";
        command.Parameters.AddWithValue("$recordCode", recordCode);

        var json = await command.ExecuteScalarAsync(cancellationToken) as string;
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<TestRecordAggregate>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<TestRecordAggregate>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT AggregateJson
FROM TestRecords
ORDER BY TestTime DESC
LIMIT $take;";
        command.Parameters.AddWithValue("$take", take);

        var records = new List<TestRecordAggregate>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var json = reader.GetString(0);
            var record = JsonSerializer.Deserialize<TestRecordAggregate>(json, _jsonOptions);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return records;
    }
}

public sealed class SQLiteRecordAttachmentRepository : IRecordAttachmentRepository
{
    private readonly string _dbPath;

    public SQLiteRecordAttachmentRepository(string? dbPath = null)
    {
        _dbPath = dbPath ?? SQLiteTestPersistence.DefaultDbPath;
        SQLiteTestPersistence.EnsureCreated(_dbPath);
    }

    public Task SaveForRecordAsync(Guid recordId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default)
        => SaveAsync("record", recordId, attachments, cancellationToken);

    public Task SaveForRecordItemAsync(Guid recordItemId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default)
        => SaveAsync("record-item", recordItemId, attachments, cancellationToken);

    public Task<IReadOnlyList<RecordAttachment>> ListForRecordAsync(Guid recordId, CancellationToken cancellationToken = default)
        => ListAsync("record", recordId, cancellationToken);

    public Task<IReadOnlyList<RecordAttachment>> ListForRecordItemAsync(Guid recordItemId, CancellationToken cancellationToken = default)
        => ListAsync("record-item", recordItemId, cancellationToken);

    private async Task SaveAsync(string ownerType, Guid ownerId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        foreach (var attachment in attachments)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO TestRecordAttachments (
    AttachmentId, OwnerType, OwnerId, FileName, FileExt, StoragePath, Length, UploadedAt, ExtraInfoJson)
VALUES (
    $attachmentId, $ownerType, $ownerId, $fileName, $fileExt, $storagePath, $length, $uploadedAt, $extraInfoJson)
ON CONFLICT(AttachmentId) DO UPDATE SET
    OwnerType = excluded.OwnerType,
    OwnerId = excluded.OwnerId,
    FileName = excluded.FileName,
    FileExt = excluded.FileExt,
    StoragePath = excluded.StoragePath,
    Length = excluded.Length,
    UploadedAt = excluded.UploadedAt,
    ExtraInfoJson = excluded.ExtraInfoJson;";
            command.Parameters.AddWithValue("$attachmentId", attachment.AttachmentId.ToString());
            command.Parameters.AddWithValue("$ownerType", ownerType);
            command.Parameters.AddWithValue("$ownerId", ownerId.ToString());
            command.Parameters.AddWithValue("$fileName", attachment.FileName);
            command.Parameters.AddWithValue("$fileExt", attachment.FileExt);
            command.Parameters.AddWithValue("$storagePath", attachment.StoragePath);
            command.Parameters.AddWithValue("$length", attachment.Length);
            command.Parameters.AddWithValue("$uploadedAt", attachment.UploadedAt.ToString("O"));
            command.Parameters.AddWithValue("$extraInfoJson", (object?)attachment.ExtraInfoJson ?? DBNull.Value);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task<IReadOnlyList<RecordAttachment>> ListAsync(string ownerType, Guid ownerId, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT AttachmentId, FileName, FileExt, StoragePath, Length, UploadedAt, ExtraInfoJson
FROM TestRecordAttachments
WHERE OwnerType = $ownerType AND OwnerId = $ownerId
ORDER BY UploadedAt ASC;";
        command.Parameters.AddWithValue("$ownerType", ownerType);
        command.Parameters.AddWithValue("$ownerId", ownerId.ToString());

        var attachments = new List<RecordAttachment>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            attachments.Add(new RecordAttachment
            {
                AttachmentId = Guid.Parse(reader.GetString(0)),
                FileName = reader.GetString(1),
                FileExt = reader.GetString(2),
                StoragePath = reader.GetString(3),
                Length = reader.GetInt64(4),
                UploadedAt = DateTimeOffset.Parse(reader.GetString(5)),
                ExtraInfoJson = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }

        return attachments;
    }
}

public sealed class SQLiteTestReportRepository : ITestReportRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = false };

    public SQLiteTestReportRepository(string? dbPath = null)
    {
        _dbPath = dbPath ?? SQLiteTestPersistence.DefaultDbPath;
        SQLiteTestPersistence.EnsureCreated(_dbPath);
    }

    public async Task SaveAsync(TestReportDocument document, string format, string content, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO TestReports (RecordCode, Format, SavedAt, Content)
VALUES ($recordCode, $format, $savedAt, $content)
ON CONFLICT(RecordCode, Format) DO UPDATE SET
    SavedAt = excluded.SavedAt,
    Content = excluded.Content;";
        command.Parameters.AddWithValue("$recordCode", document.RecordCode);
        command.Parameters.AddWithValue("$format", format);
        command.Parameters.AddWithValue("$savedAt", DateTimeOffset.Now.ToString("O"));
        command.Parameters.AddWithValue("$content", string.IsNullOrWhiteSpace(content) ? JsonSerializer.Serialize(document, _jsonOptions) : content);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveSummaryAsync(TestReportPersistenceSummary summary, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO TestReportSummaries (RecordCode, Format, ArtifactFileName, ArtifactSavedPath, ExportedAt, ContentLength)
VALUES ($recordCode, $format, $artifactFileName, $artifactSavedPath, $exportedAt, $contentLength)
ON CONFLICT(RecordCode, Format) DO UPDATE SET
    ArtifactFileName = excluded.ArtifactFileName,
    ArtifactSavedPath = excluded.ArtifactSavedPath,
    ExportedAt = excluded.ExportedAt,
    ContentLength = excluded.ContentLength;";
        command.Parameters.AddWithValue("$recordCode", summary.RecordCode);
        command.Parameters.AddWithValue("$format", summary.Format);
        command.Parameters.AddWithValue("$artifactFileName", summary.ArtifactFileName);
        command.Parameters.AddWithValue("$artifactSavedPath", summary.ArtifactSavedPath);
        command.Parameters.AddWithValue("$exportedAt", summary.ExportedAt.ToString("O"));
        command.Parameters.AddWithValue("$contentLength", summary.ContentLength);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestReportSnapshot>> ListForRecordCodeAsync(string recordCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT r.RecordCode, r.Format, r.SavedAt, r.Content,
       COALESCE(s.ArtifactFileName, ''),
       COALESCE(s.ArtifactSavedPath, '')
FROM TestReports r
LEFT JOIN TestReportSummaries s
  ON s.RecordCode = r.RecordCode AND s.Format = r.Format
WHERE r.RecordCode = $recordCode
ORDER BY r.SavedAt DESC;";
        command.Parameters.AddWithValue("$recordCode", recordCode);

        var snapshots = new List<TestReportSnapshot>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            snapshots.Add(new TestReportSnapshot
            {
                RecordCode = reader.GetString(0),
                Format = reader.GetString(1),
                SavedAt = DateTimeOffset.Parse(reader.GetString(2)),
                Content = reader.GetString(3),
                ArtifactFileName = reader.GetString(4),
                ArtifactSavedPath = reader.GetString(5)
            });
        }

        return snapshots;
    }

    public async Task<IReadOnlyList<TestReportPersistenceSummary>> ListRecentSummariesAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT RecordCode, Format, ArtifactFileName, ArtifactSavedPath, ExportedAt, ContentLength
FROM TestReportSummaries
ORDER BY ExportedAt DESC
LIMIT $take;";
        command.Parameters.AddWithValue("$take", take);

        var summaries = new List<TestReportPersistenceSummary>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            summaries.Add(new TestReportPersistenceSummary
            {
                RecordCode = reader.GetString(0),
                Format = reader.GetString(1),
                ArtifactFileName = reader.GetString(2),
                ArtifactSavedPath = reader.GetString(3),
                ExportedAt = DateTimeOffset.Parse(reader.GetString(4)),
                ContentLength = reader.GetInt32(5)
            });
        }

        return summaries;
    }
}
