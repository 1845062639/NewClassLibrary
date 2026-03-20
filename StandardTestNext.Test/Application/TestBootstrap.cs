using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Infrastructure.Persistence;
using System.Linq;

namespace StandardTestNext.Test.Application;

public sealed class TestBootstrap
{
    public void Run(IMessageBus messageBus, TestStartupOptions? options = null)
    {
        Console.WriteLine("StandardTestNext.Test starting...");

        options ??= new TestStartupOptions();

        var sessionService = new MotorTestSessionService();
        var rated = sessionService.BuildDemoRatedParams();
        sessionService.PrintReadyState();

        var runtime = new TestRuntimeOrchestrator(messageBus, new TestCommandBuilder());
        var command = runtime.StartDemoSession("Motor_Y");
        Console.WriteLine($"[Test] Command published: {command.CommandName}");

        var samples = new[]
        {
            new StandardTestNext.Contracts.MotorRealtimeSampleContract
            {
                SampleTime = DateTimeOffset.Now,
                DeviceId = "mock-motor-01",
                ProductKind = rated.ProductKind,
                VoltageAverage = 380,
                CurrentAverage = 21.8,
                Power = 10.6,
                Frequency = 50,
                Speed = 1472,
                Torque = 68.4,
                IsRecordPoint = true
            },
            new StandardTestNext.Contracts.MotorRealtimeSampleContract
            {
                SampleTime = DateTimeOffset.Now.AddSeconds(1),
                DeviceId = "mock-motor-01",
                ProductKind = rated.ProductKind,
                VoltageAverage = 379.5,
                CurrentAverage = 21.9,
                Power = 10.7,
                Frequency = 50,
                Speed = 1471,
                Torque = 68.8,
                IsRecordPoint = false
            }
        };

        var aggregateBuilder = new TestRecordAggregateBuilder();

        var persistenceMode = NormalizePersistenceMode(options.PersistenceMode);
        Console.WriteLine($"[Test] Persistence mode: {persistenceMode}");
        if (!string.IsNullOrWhiteSpace(options.SQLiteDbPath))
        {
            Console.WriteLine($"[Test] SQLite db override: {options.SQLiteDbPath}");
        }

        IProductDefinitionRepository productRepository;
        ITestRecordRepository recordRepository;
        IRecordAttachmentRepository attachmentRepository;
        ITestReportRepository reportRepository;

        if (string.Equals(persistenceMode, "sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var sqliteDbPath = options.SQLiteDbPath;
            SQLiteTestPersistence.EnsureCreated(sqliteDbPath);
            Console.WriteLine($"[Test] SQLite db: {sqliteDbPath ?? SQLiteTestPersistence.DefaultDbPath}");

            productRepository = new SQLiteProductDefinitionRepository(sqliteDbPath);
            recordRepository = new SQLiteTestRecordRepository(sqliteDbPath);
            attachmentRepository = new SQLiteRecordAttachmentRepository(sqliteDbPath);
            reportRepository = new SQLiteTestReportRepository(sqliteDbPath);
        }
        else
        {
            productRepository = new InMemoryProductDefinitionRepository();
            recordRepository = new InMemoryTestRecordRepository();
            attachmentRepository = new InMemoryRecordAttachmentRepository();
            reportRepository = new InMemoryTestReportRepository();
        }

        ITestReportArtifactWriter reportArtifactWriter = new FileSystemTestReportArtifactWriter();
        ITestRecordQueryService recordQueryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
        ITestReportQueryService reportQueryService = new TestReportQueryService(reportRepository);
        ITestProductDefinitionService productDefinitionService = new TestProductDefinitionService(productRepository);
        IProductDefinitionQueryService productDefinitionQueryService = new ProductDefinitionQueryService(productRepository);

        var productDefinition = productDefinitionService.GetOrCreateAsync(rated).GetAwaiter().GetResult();
        var buildResult = aggregateBuilder.BuildDemoRecord(rated, samples, productDefinition);
        var aggregate = buildResult.Record;

        recordRepository.SaveAsync(aggregate).GetAwaiter().GetResult();
        attachmentRepository.SaveForRecordAsync(aggregate.TestRecordId, aggregate.Attachments).GetAwaiter().GetResult();
        foreach (var item in aggregate.Items)
        {
            attachmentRepository.SaveForRecordItemAsync(item.TestRecordItemId, item.Attachments).GetAwaiter().GetResult();
        }

        var reportExporter = new TestReportExportService();
        var reportDocument = reportExporter.BuildDocument(aggregate, buildResult.Statistics);
        var reportArtifacts = new[]
        {
            reportExporter.ExportAndWriteAsync(reportDocument, new JsonTestReportRenderer(), reportArtifactWriter).GetAwaiter().GetResult(),
            reportExporter.ExportAndWriteAsync(reportDocument, new MarkdownTestReportRenderer(), reportArtifactWriter).GetAwaiter().GetResult()
        };

        foreach (var reportArtifact in reportArtifacts)
        {
            reportRepository.SaveAsync(reportDocument, reportArtifact.Format, reportArtifact.Content).GetAwaiter().GetResult();

            var reportSummary = new TestReportPersistenceSummary
            {
                RecordCode = reportDocument.RecordCode,
                Format = reportArtifact.Format,
                ArtifactFileName = reportArtifact.Artifact.FileName,
                ArtifactSavedPath = reportArtifact.Artifact.SavedPath,
                ExportedAt = reportArtifact.Artifact.WrittenAt,
                ContentLength = reportArtifact.Content.Length
            };
            reportRepository.SaveSummaryAsync(reportSummary).GetAwaiter().GetResult();
        }

        var primaryReport = reportArtifacts.First(x => string.Equals(x.Format, "json", StringComparison.OrdinalIgnoreCase));

        var recentRecords = recordQueryService.ListRecentAsync(5).GetAwaiter().GetResult();
        var reloadedRecord = recordQueryService.GetByRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recordReports = reportQueryService.ListForRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recentReportSummaries = reportQueryService.ListRecentSummariesAsync(5).GetAwaiter().GetResult();
        var recentProducts = productDefinitionQueryService.ListRecentAsync(5).GetAwaiter().GetResult();
        var reloadedProductDefinition = productDefinitionQueryService.GetByKindAsync(rated.ProductKind).GetAwaiter().GetResult();

        Console.WriteLine($"[Test] Product definition resolved: {productDefinition.ProductId} / {productDefinition.ProductKind} / {productDefinition.Model}");
        Console.WriteLine($"[Test] Aggregate persisted: record={aggregate.TestRecordId}, items={aggregate.Items.Count}");
        Console.WriteLine($"[Test] Mapping partitions: {string.Join(", ", buildResult.Mapping.Partitions.Select(x => $"{x.RecordMode}:{x.SampleCount}"))}");
        Console.WriteLine($"[Test] Reports persisted: {string.Join(", ", reportArtifacts.Select(x => $"{x.Format}:{x.Artifact.FileName}"))}");
        Console.WriteLine($"[Test] Primary report artifact: {primaryReport.Artifact.FileName} -> {primaryReport.Artifact.SavedPath}");
        Console.WriteLine($"[Test] Recent records: {string.Join(", ", recentRecords.Select(x => $"{x.RecordCode}:{x.ProductModel ?? x.ProductKind}:reused={(x.ReusedProductDefinition ? "Y" : "N")}:reports={x.ReportCount}:artifacts={(x.HasReportArtifacts ? "Y" : "N")}"))}");
        Console.WriteLine($"[Test] Record reports: {string.Join(", ", recordReports.Select(x => $"{x.RecordCode}:{x.Format}:{x.ArtifactFileName}"))}");
        Console.WriteLine($"[Test] Recent report summaries: {string.Join(", ", recentReportSummaries.Select(x => $"{x.RecordCode}:{x.Format}"))}");
        Console.WriteLine($"[Test] Recent products: {string.Join(", ", recentProducts.Select(x => $"{x.ProductKind}:{x.Model}:{x.Code}"))}");
        Console.WriteLine($"[Test] Reloaded product definition found: {reloadedProductDefinition is not null}");
        Console.WriteLine($"[Test] Reloaded record found: {reloadedRecord is not null}");
        if (reloadedRecord is not null)
        {
            Console.WriteLine($"[Test] Reloaded record attachments: record={reloadedRecord.RecordAttachments.Count}, itemBuckets={reloadedRecord.ItemAttachments.Count}");
            Console.WriteLine($"[Test] Reloaded item details: {string.Join(", ", reloadedRecord.ItemDetails.Select(x => $"{x.ItemCode}:{x.RecordMode}:{x.SampleCount}:remark={(x.HasRemark ? "Y" : "N")}"))}");
            Console.WriteLine($"[Test] Reloaded reports: snapshots={reloadedRecord.Reports.Count}, summaries={reloadedRecord.ReportSummaries.Count}, hasArtifacts={reloadedRecord.HasReportArtifacts}");
        }
        new TestRecordConsolePresenter().PrintSummary(aggregate, buildResult.Statistics, primaryReport.Format, primaryReport.Content);

        Console.WriteLine("StandardTestNext.Test ready.");
    }

    private static string NormalizePersistenceMode(string? mode)
    {
        return string.IsNullOrWhiteSpace(mode)
            ? "memory"
            : mode.Trim().ToLowerInvariant();
    }
}
