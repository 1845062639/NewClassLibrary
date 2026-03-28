
using StandardTestNext.Contracts;
using StandardTestNext.Test.RuntimeBridge;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.AppSide;
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

        var appRecordQueryFacade = new TestRecordQueryFacade(recordQueryService);
        ITestReportQueryService reportQueryService = new TestReportQueryService(reportRepository);
        ITestProductDefinitionService productDefinitionService = new TestProductDefinitionService(productRepository);
        IProductDefinitionQueryService productDefinitionQueryService = new ProductDefinitionQueryService(productRepository);

        var productDefinition = productDefinitionService.GetOrCreateAsync(rated).GetAwaiter().GetResult();
        var buildResult = aggregateBuilder.BuildDemoRecord(rated, samples, legacySamples: null, productDefinition);
        var aggregate = buildResult.Record;

        recordRepository.SaveAsync(aggregate).GetAwaiter().GetResult();
        attachmentRepository.SaveForRecordAsync(aggregate.TestRecordId, aggregate.Attachments).GetAwaiter().GetResult();
        foreach (var item in aggregate.Items)
        {
            attachmentRepository.SaveForRecordItemAsync(item.TestRecordItemId, item.Attachments).GetAwaiter().GetResult();
        }

        var reportExporter = new TestReportExportService();
        var reportPersistenceService = new TestReportPersistenceService();
        var reportDocument = reportExporter.BuildDocument(aggregate, buildResult.Statistics);
        var reportArtifacts = new[]
        {
            reportPersistenceService.ExportWriteAndSaveAsync(reportDocument, new JsonTestReportRenderer(), reportArtifactWriter, reportRepository, isPrimary: true, isLightweight: false).GetAwaiter().GetResult(),
            reportPersistenceService.ExportWriteAndSaveAsync(reportDocument, new MarkdownTestReportRenderer(), reportArtifactWriter, reportRepository, isPrimary: false, isLightweight: false).GetAwaiter().GetResult(),
            reportPersistenceService.ExportWriteAndSaveAsync(reportDocument, new ManifestTestReportRenderer(), reportArtifactWriter, reportRepository, isPrimary: false, isLightweight: true).GetAwaiter().GetResult()
        };

        var primaryReport = reportArtifacts.First(x => string.Equals(x.Format, "json", StringComparison.OrdinalIgnoreCase));

        var recentRecords = recordQueryService.ListRecentAsync(5).GetAwaiter().GetResult();
        var recentRecordViews = appRecordQueryFacade.ListRecentForAppAsync(5).GetAwaiter().GetResult();
        var reloadedRecord = recordQueryService.GetByRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var reloadedRecordView = appRecordQueryFacade.GetDetailForAppAsync(aggregate.RecordCode).GetAwaiter().GetResult();

        var appQueryGateway = new TestRecordQueryGatewayAdapter(appRecordQueryFacade);
        var appRecentRecords = appQueryGateway.ListRecentAsync(5).GetAwaiter().GetResult();
        var appRecordDetail = appQueryGateway.GetDetailAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recordReports = reportQueryService.ListForRecordCodeAsync(aggregate.RecordCode).GetAwaiter().GetResult();
        var recentReportSummaries = reportQueryService.ListRecentSummariesAsync(5).GetAwaiter().GetResult();
        var recentProducts = productDefinitionQueryService.ListRecentAsync(5).GetAwaiter().GetResult();
        var reloadedProductDefinition = productDefinitionQueryService.GetByKindAsync(rated.ProductKind).GetAwaiter().GetResult();
        var lightweightReport = recordReports.FirstOrDefault(x => x.IsLightweightEntry);
        var primaryRecordReport = recordReports.FirstOrDefault(x => x.IsPrimaryEntry);
        var noLoadPayload = aggregate.Items.FirstOrDefault(x => x.ItemCode == "MotorY.NoLoad")?.DataJson;
        var noLoadLegacyShape = MotorYNoLoadLegacyShape.FromJson(noLoadPayload ?? string.Empty);
        var lockRotorPayload = aggregate.Items.FirstOrDefault(x => x.ItemCode == "MotorY.LockedRotor")?.DataJson;
        var lockRotorLegacyShape = MotorYLockRotorLegacyShape.FromJson(lockRotorPayload ?? string.Empty);
        var thermalPayload = aggregate.Items.FirstOrDefault(x => x.ItemCode == "MotorY.HeatRun")?.DataJson;
        var thermalLegacyShape = MotorYThermalLegacyShape.FromJson(thermalPayload ?? string.Empty);
        var loadAPayload = aggregate.Items.FirstOrDefault(x => x.ItemCode == "MotorY.LoadA")?.DataJson;
        var loadALegacyShape = MotorYLoadALegacyShape.FromJson(loadAPayload ?? string.Empty);
        var loadBPayload = aggregate.Items.FirstOrDefault(x => x.ItemCode == "MotorY.LoadB")?.DataJson;
        var loadBLegacyShape = MotorYLoadBLegacyShape.FromJson(loadBPayload ?? string.Empty);
        var normalizedRealStpAliases = new[]
        {
            "直流电阻测定",
            "陪试直流电阻测定",
            "空载特性试验",
            "空载试验",
            "空载试验（出厂）",
            "空载特性测量",
            "空载特性完全试验",
            "热试验",
            "热试验2",
            "温度计法热试验",
            "A法负载试验",
            "B法负载试验",
            "堵转特性试验",
            "堵转试验",
            "堵转试验（出厂）",
            "C法负载试验"
        }
            .Select(code => $"{code}->{MotorYLegacyItemCodeNormalizer.Normalize(code)}:core={(MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(code) ? "Y" : "N")}")
            .ToArray();

        Console.WriteLine($"[Test] Product definition resolved: {productDefinition.ProductId} / {productDefinition.ProductKind} / {productDefinition.Model}");
        Console.WriteLine($"[Test] Aggregate persisted: record={aggregate.TestRecordId}, items={aggregate.Items.Count}");
        Console.WriteLine($"[Test] Mapping partitions: {string.Join(", ", buildResult.Mapping.Partitions.Select(x => $"{x.RecordMode}:{x.SampleCount}"))}");
        Console.WriteLine($"[Test] Reports persisted: {string.Join(", ", reportArtifacts.Select(x => $"{x.Format}:{x.Artifact.FileName}"))}");
        Console.WriteLine($"[Test] Primary report artifact: {primaryReport.Artifact.FileName} -> {primaryReport.Artifact.SavedPath}");
        Console.WriteLine($"[Test] Recent records: {string.Join(", ", recentRecords.Select(x => $"{x.RecordCode}:{x.ProductModel ?? x.ProductKind}:reused={(x.ReusedProductDefinition ? "Y" : "N")}:reports={x.ReportCount}:artifacts={(x.HasReportArtifacts ? "Y" : "N")}:primary={x.PrimaryReportFormat ?? "<none>"}:{x.PrimaryReportArtifactFileName ?? "<none>"}:light={x.LightweightReportFormat ?? "<none>"}:{x.LightweightReportArtifactFileName ?? "<none>"}:samples={x.Mapping.TotalSampleCount}:kp={x.Mapping.KeyPointSampleCount}:cont={x.Mapping.ContinuousSampleCount}"))}");
        Console.WriteLine($"[Test] Recent record views: {string.Join(", ", recentRecordViews.Select(x => $"{x.RecordCode}:{x.ProductDisplayName}:items={x.ItemCount}:samples={x.SampleCount}:attachments={x.RecordAttachmentCount}/{x.ItemAttachmentBucketCount}:reports={x.ReportCount}:primary={x.PrimaryReportFormat ?? "<none>"}:{x.PrimaryReportArtifactFileName ?? "<none>"}:light={x.LightweightReportFormat ?? "<none>"}:{x.LightweightReportArtifactFileName ?? "<none>"}"))}");
        Console.WriteLine($"[Test] App query gateway list: {string.Join(", ", appRecentRecords.Select(x => $"{x.RecordCode}:{x.ProductDisplayName}:items={x.ItemCount}:samples={x.SampleCount}:attachments={x.RecordAttachmentCount}/{x.ItemAttachmentBucketCount}:reports={x.ReportCount}:primary={x.PrimaryReportFormat ?? "<none>"}:{x.PrimaryReportArtifactFileName ?? "<none>"}:light={x.LightweightReportFormat ?? "<none>"}:{x.LightweightReportArtifactFileName ?? "<none>"}:partitions={string.Join("|", x.ItemPartitions.Select(p => $"{p.ItemCode}:{p.SampleCount}"))}"))}");
        Console.WriteLine($"[Test] App query gateway detail found: {appRecordDetail is not null}");
        Console.WriteLine($"[Test] Record reports: {string.Join(", ", recordReports.Select(x => $"{x.RecordCode}:{x.Format}:{x.ArtifactFileName}:light={(x.IsLightweightEntry ? "Y" : "N")}:primary={(x.IsPrimaryEntry ? "Y" : "N")}"))}");
        Console.WriteLine($"[Test] Lightweight report artifact: {(lightweightReport is null ? "<none>" : $"{lightweightReport.Format}:{lightweightReport.ArtifactFileName}")}");
        Console.WriteLine($"[Test] Primary record report: {(primaryRecordReport is null ? "<none>" : $"{primaryRecordReport.Format}:{primaryRecordReport.ArtifactFileName}")}");
        Console.WriteLine($"[Test] Recent report summaries: {string.Join(", ", recentReportSummaries.Select(x => $"{x.RecordCode}:{x.Format}:light={(x.IsLightweightEntry ? "Y" : "N")}:primary={(x.IsPrimaryEntry ? "Y" : "N")}"))}");
        Console.WriteLine($"[Test] Recent products: {string.Join(", ", recentProducts.Select(x => $"{x.ProductKind}:{x.Model}:{x.Code}"))}");
        Console.WriteLine($"[Test] NoLoad legacy-shape preview: {MotorYNoLoadLegacyPreviewFormatter.Format(noLoadLegacyShape)}");
        Console.WriteLine($"[Test] LockRotor legacy-shape preview: {MotorYLegacyShapePreviewFormatter.FormatLockRotor(lockRotorLegacyShape)}");
        Console.WriteLine($"[Test] Thermal legacy-shape preview: {MotorYLegacyShapePreviewFormatter.FormatThermal(thermalLegacyShape)}");
        Console.WriteLine($"[Test] LoadA legacy-shape preview: {MotorYLegacyShapePreviewFormatter.FormatLoadA(loadALegacyShape)}");
        Console.WriteLine($"[Test] LoadB legacy-shape preview: {MotorYLegacyShapePreviewFormatter.FormatLoadB(loadBLegacyShape)}");
        Console.WriteLine($"[Test] Real stp alias normalization: {string.Join(", ", normalizedRealStpAliases)}");
        Console.WriteLine($"[Test] Reloaded product definition found: {reloadedProductDefinition is not null}");
        Console.WriteLine($"[Test] Reloaded record found: {reloadedRecord is not null}");
        if (reloadedRecord is not null)
        {
            Console.WriteLine($"[Test] Reloaded record attachments: record={reloadedRecord.RecordAttachments.Count}, itemBuckets={reloadedRecord.ItemAttachments.Count}");
            Console.WriteLine($"[Test] Reloaded item details: {string.Join(", ", reloadedRecord.ItemDetails.Select(x => $"{x.ItemCode}:{x.RecordMode}:{x.SampleCount}:legacy={x.LegacySampleCount}:power={x.LegacyPayload.PowerCurveImageCount}:temp={x.LegacyPayload.TempCurveImageCount}:vibration={x.LegacyPayload.VibrationCurveImageCount}:remark={(x.HasRemark ? "Y" : "N")}"))}");
            Console.WriteLine($"[Test] Reloaded mapping: samples={reloadedRecord.Mapping.TotalSampleCount}:kp={reloadedRecord.Mapping.KeyPointSampleCount}:cont={reloadedRecord.Mapping.ContinuousSampleCount}");
            Console.WriteLine($"[Test] Reloaded reports: snapshots={reloadedRecord.Reports.Count}, summaries={reloadedRecord.ReportSummaries.Count}, hasArtifacts={reloadedRecord.HasReportArtifacts}");
            if (reloadedRecordView is not null)
            {
                Console.WriteLine($"[Test] Reloaded record view: {reloadedRecordView.RecordCode}:{reloadedRecordView.ProductDisplayName}:items={reloadedRecordView.ItemCount}:samples={reloadedRecordView.SampleCount}:reports={reloadedRecordView.ReportSummaries.Count}");
                if (reloadedRecordView.MotorYMethodDecisions.Count > 0)
                {
                    Console.WriteLine($"[Test] Reloaded Motor_Y method decisions: {FormatMethodDecisions(reloadedRecordView.MotorYMethodDecisions)}");
                }
            }
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

    private static string FormatMethodDecisions(IReadOnlyList<MotorYMethodDecisionSnapshot> decisions)
    {
        return string.Join(", ", decisions.Select(FormatMethodDecisionSnapshot));
    }

    private static string FormatMethodDecisionSnapshot(MotorYMethodDecisionSnapshot decision)
    {
        var baseline = decision.BaselineRoute is null
            ? "baseline=<none>"
            : $"baseline={decision.BaselineRoute.MethodValue}/{decision.BaselineRoute.ProfileKey}:{decision.BaselineCount}";
        var dominant = decision.DominantRoute is null
            ? "dominant=<none>"
            : $"dominant={decision.DominantRoute.MethodValue}/{decision.DominantRoute.ProfileKey}:{decision.DominantCount}:{decision.DominantShare:P1}";
        var distributions = decision.Distributions.Count == 0
            ? "dist=<none>"
            : "dist=" + string.Join("|", decision.Distributions.Select(x => $"{x.MethodValue}:{x.Count}:{x.Share:P1}:{x.Route?.VariantKind ?? "unknown"}"));

        return $"{decision.CanonicalCode}[{baseline};{dominant};prioritize={(decision.ShouldPrioritizeDominantOverBaseline ? "Y" : "N")};{distributions}]";
    }
}
