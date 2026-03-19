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
        var buildResult = aggregateBuilder.BuildDemoRecord(rated, samples);
        var aggregate = buildResult.Record;

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

        if (aggregate.TestProduct is not null)
        {
            productRepository.SaveAsync(aggregate.TestProduct).GetAwaiter().GetResult();
        }

        recordRepository.SaveAsync(aggregate).GetAwaiter().GetResult();
        attachmentRepository.SaveForRecordAsync(aggregate.TestRecordId, aggregate.Attachments).GetAwaiter().GetResult();
        foreach (var item in aggregate.Items)
        {
            attachmentRepository.SaveForRecordItemAsync(item.TestRecordItemId, item.Attachments).GetAwaiter().GetResult();
        }

        var reportExporter = new TestReportExportService();
        var reportDocument = reportExporter.BuildDocument(aggregate, buildResult.Statistics);
        var persistedReport = reportExporter.ExportAndWriteAsync(
            reportDocument,
            new JsonTestReportRenderer(),
            reportArtifactWriter).GetAwaiter().GetResult();
        reportRepository.SaveAsync(reportDocument, persistedReport.Format, persistedReport.Content).GetAwaiter().GetResult();

        var reportSummary = new TestReportPersistenceSummary
        {
            RecordCode = reportDocument.RecordCode,
            Format = persistedReport.Format,
            ArtifactFileName = persistedReport.Artifact.FileName,
            ArtifactSavedPath = persistedReport.Artifact.SavedPath,
            ExportedAt = persistedReport.Artifact.WrittenAt,
            ContentLength = persistedReport.Content.Length
        };
        reportRepository.SaveSummaryAsync(reportSummary).GetAwaiter().GetResult();

        var recentRecords = recordQueryService.ListRecentAsync(5).GetAwaiter().GetResult();
        var reloadedRecord = recordQueryService.GetByRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recordReports = reportQueryService.ListForRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recentReportSummaries = reportQueryService.ListRecentSummariesAsync(5).GetAwaiter().GetResult();

        Console.WriteLine($"[Test] Aggregate persisted: record={aggregate.TestRecordId}, items={aggregate.Items.Count}");
        Console.WriteLine($"[Test] Mapping partitions: {string.Join(", ", buildResult.Mapping.Partitions.Select(x => $"{x.RecordMode}:{x.SampleCount}"))}");
        Console.WriteLine($"[Test] Report persisted: recordCode={reportDocument.RecordCode}, format={persistedReport.Format}");
        Console.WriteLine($"[Test] Report artifact: {persistedReport.Artifact.FileName} -> {persistedReport.Artifact.SavedPath}");
        Console.WriteLine($"[Test] Report summary: exportedAt={reportSummary.ExportedAt:O}, contentLength={reportSummary.ContentLength}");
        Console.WriteLine($"[Test] Recent records: {string.Join(", ", recentRecords.Select(x => x.RecordCode))}");
        Console.WriteLine($"[Test] Record reports: {string.Join(", ", recordReports.Select(x => $"{x.RecordCode}:{x.Format}:{x.ArtifactFileName}"))}");
        Console.WriteLine($"[Test] Recent report summaries: {string.Join(", ", recentReportSummaries.Select(x => $"{x.RecordCode}:{x.Format}"))}");
        Console.WriteLine($"[Test] Reloaded record found: {reloadedRecord is not null}");
        if (reloadedRecord is not null)
        {
            Console.WriteLine($"[Test] Reloaded record attachments: record={reloadedRecord.RecordAttachments.Count}, itemBuckets={reloadedRecord.ItemAttachments.Count}");
            Console.WriteLine($"[Test] Reloaded item details: {string.Join(", ", reloadedRecord.ItemDetails.Select(x => $"{x.ItemCode}:{x.RecordMode}:{x.SampleCount}:remark={(x.HasRemark ? "Y" : "N")}"))}");
            Console.WriteLine($"[Test] Reloaded reports: snapshots={reloadedRecord.Reports.Count}, summaries={reloadedRecord.ReportSummaries.Count}, hasArtifacts={reloadedRecord.HasReportArtifacts}");
        }
        new TestRecordConsolePresenter().PrintSummary(aggregate, buildResult.Statistics, persistedReport.Format, persistedReport.Content);

        Console.WriteLine("StandardTestNext.Test ready.");
    }

    private static string NormalizePersistenceMode(string? mode)
    {
        return string.IsNullOrWhiteSpace(mode)
            ? "memory"
            : mode.Trim().ToLowerInvariant();
    }
}
