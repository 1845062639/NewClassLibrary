
using StandardTestNext.Contracts;
using StandardTestNext.Test.RuntimeBridge;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.AppSide;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Infrastructure.Persistence;
using System.Globalization;
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
        var stpDbSnapshotQueryService = new StpDbSnapshotQueryService();

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
        var stpMethodAdaptationPlans = LoadStpMethodAdaptationPlans(stpDbSnapshotQueryService);
        var stpCrossPlanPrimaryFieldFocuses = BuildCrossPlanDecisionAnchorPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
        var stpCrossPlanRequiredResultPrimaryFieldFocuses = BuildCrossPlanRequiredResultPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
        var stpAlgorithmFamilyAnchorPrimaryFieldFocuses = BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
        var stpVariantKindAnchorPrimaryFieldFocuses = BuildVariantKindDecisionAnchorPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
        var stpAlgorithmFamilyRequiredResultPrimaryFieldFocuses = BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
        var stpVariantKindRequiredResultPrimaryFieldFocuses = BuildVariantKindRequiredResultPrimaryFieldFocuses(stpMethodAdaptationPlans, stpDbSnapshotQueryService);
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
        if (stpMethodAdaptationPlans.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y adaptation plans: {FormatMethodAdaptationPlans(stpMethodAdaptationPlans)}");
        }
        if (stpCrossPlanPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y cross-plan anchor primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpCrossPlanPrimaryFieldFocuses)}");
        }
        if (stpCrossPlanRequiredResultPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y cross-plan required-result primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpCrossPlanRequiredResultPrimaryFieldFocuses)}");
        }
        if (stpAlgorithmFamilyAnchorPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y algorithm-family anchor primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpAlgorithmFamilyAnchorPrimaryFieldFocuses)}");
        }
        if (stpAlgorithmFamilyRequiredResultPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y algorithm-family required-result primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpAlgorithmFamilyRequiredResultPrimaryFieldFocuses)}");
        }
        if (stpVariantKindAnchorPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y variant-kind anchor primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpVariantKindAnchorPrimaryFieldFocuses)}");
        }
        if (stpVariantKindRequiredResultPrimaryFieldFocuses.Count > 0)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y variant-kind required-result primary fields: {FormatCrossPlanPrimaryFieldFocuses(stpVariantKindRequiredResultPrimaryFieldFocuses)}");
        }
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

    private static IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> LoadStpMethodAdaptationPlans(StpDbSnapshotQueryService snapshotQueryService)
    {
        try
        {
            return snapshotQueryService.ListMotorYMethodAdaptationPlans();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y adaptation plans unavailable: {ex.Message}");
            return Array.Empty<MotorYMethodAdaptationPlanSnapshot>();
        }
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanDecisionAnchorPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> stpMethodAdaptationPlans,
        StpDbSnapshotQueryService snapshotQueryService)
    {
        if (stpMethodAdaptationPlans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        try
        {
            return snapshotQueryService.ListMotorYDecisionAnchorPrimaryFieldFocuses();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y cross-plan anchor primary fields unavailable: {ex.Message}");
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanRequiredResultPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> stpMethodAdaptationPlans,
        StpDbSnapshotQueryService snapshotQueryService)
    {
        if (stpMethodAdaptationPlans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        try
        {
            return snapshotQueryService.ListMotorYRequiredResultPrimaryFieldFocuses();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y cross-plan required-result primary fields unavailable: {ex.Message}");
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> stpMethodAdaptationPlans,
        StpDbSnapshotQueryService snapshotQueryService)
    {
        if (stpMethodAdaptationPlans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        try
        {
            return snapshotQueryService.ListMotorYAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y algorithm-family anchor primary fields unavailable: {ex.Message}");
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> stpMethodAdaptationPlans,
        StpDbSnapshotQueryService snapshotQueryService)
    {
        if (stpMethodAdaptationPlans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        try
        {
            return snapshotQueryService.ListMotorYAlgorithmFamilyRequiredResultPrimaryFieldFocuses();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test] stp.db Motor_Y algorithm-family required-result primary fields unavailable: {ex.Message}");
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }
    }

    private static string FormatMethodAdaptationPlans(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
    {
        return string.Join(", ", plans.Select(FormatMethodAdaptationPlanSnapshot));
    }

    private static string FormatCrossPlanPrimaryFieldFocuses(IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var totalWeighted = focuses.Count == 0
            ? 0
            : focuses
                .Select(focus => focus.WeightedShare <= 0d
                    ? 0
                    : (int)Math.Round(focus.WeightedCount / focus.WeightedShare, MidpointRounding.AwayFromZero))
                .Where(value => value > 0)
                .DefaultIfEmpty(0)
                .Max();

        return string.Join(", ", focuses.Select(focus =>
        {
            var weightedBase = totalWeighted > 0
                ? totalWeighted
                : focus.WeightedCount;
            return $"{focus.PrimaryField}:{focus.Count}:{focus.Share:P1}:weighted={focus.WeightedCount}/{weightedBase}:{focus.WeightedShare:P1}:methods={FormatPreview(focus.MethodValues.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray(), 4)}:method-keys={FormatPreview(focus.MethodKeys, 4)}:profiles={FormatPreview(focus.ProfileKeys, 4)}:legacy-methods={FormatPreview(focus.LegacyMethodNames, 4)}:settings-methods={FormatPreview(focus.SettingsMethodNames, 4)}:algo-entries={FormatPreview(focus.LegacyAlgorithmEntries, 4)}:source-sections={FormatPreview(focus.SourceSections, 4)}:source-ranges={FormatPreview(focus.SourceRanges, 4)}:forms={FormatPreview(focus.FormNames, 4)}:form-ranges={FormatPreview(focus.FormSourceRanges, 4)}:upstream={FormatPreview(focus.UpstreamCanonicalCodes, 4)}:upstream-hints={FormatPreview(focus.UpstreamSummaryHints, 2)}:{FormatPreview(focus.CanonicalCodes, 3)}:{FormatPreview(focus.AnchorKeys, 3)}:{FormatPreview(focus.SuggestedNextStepPriorities, 3)}:summary={focus.Summary}";
        }));
    }

    private static string FormatMethodAdaptationPlanSnapshot(MotorYMethodAdaptationPlanSnapshot plan)
    {
        var baseline = plan.BaselineRoute is null
            ? "baseline=<none>"
            : $"baseline={plan.BaselineRoute.MethodValue}/{plan.BaselineRoute.ProfileKey}:{plan.BaselineCount}";
        var dominant = plan.DominantRoute is null
            ? "dominant=<none>"
            : $"dominant={plan.DominantRoute.MethodValue}/{plan.DominantRoute.ProfileKey}:{plan.DominantCount}:{plan.DominantShare:P1}";
        var selected = plan.SelectedRoute is null
            ? $"selected={plan.SelectionStrategy}:<none>"
            : $"selected={plan.SelectionStrategy}:{plan.SelectedRoute.MethodValue}/{plan.SelectedRoute.ProfileKey}:{plan.SelectedCount}";
        var readiness = plan.LegacyAlgorithmInputsReady
            ? $"ready=Y:{plan.AlgorithmInputFieldCoveragePercentagePoints}pp"
            : $"ready=N:{plan.AlgorithmInputFieldCoveragePercentagePoints}pp:missing={FormatPreview(plan.MissingAlgorithmInputFields, 6)}";
        var upstream = $"upstream={(plan.UpstreamDependenciesSatisfied ? "ok" : "missing")}:{plan.ObservedUpstreamCanonicalCodeCount}/{plan.UpstreamCanonicalCodes.Count}:{FormatPreview(plan.MissingUpstreamCanonicalCodes, 3)}";
        var distributions = plan.Distributions.Count == 0
            ? "dist=<none>"
            : "dist=" + string.Join("|", plan.Distributions.Select(x => $"{x.MethodValue}:{x.Count}:{x.Share:P1}:{x.Route?.VariantKind ?? "unknown"}"));
        var sampleGates = $"samples=raw {(plan.RawSampleCountReady ? "ok" : "gap")}/{plan.RawDataSampleCount}/{plan.MinimumRawSampleCount}|payload {(plan.StructuredPayloadSampleCountReady ? "ok" : "gap")}/{plan.StructuredPayloadSampleCount}/{plan.MinimumStructuredPayloadSampleCount}|result {(plan.StructuredResultSampleCountReady ? "ok" : "gap")}/{plan.StructuredResultSampleCount}/{plan.MinimumStructuredResultSampleCount}";
        var anchors = $"anchors={(plan.LegacyDecisionAnchorReady ? "ok" : "gap")}:{plan.ResolvedLegacyDecisionAnchorCount}/{plan.LegacyDecisionAnchorResolutions.Count}:{FormatPreview(plan.MissingLegacyDecisionAnchors, 3)}";
        var coverage = $"coverage=payload {plan.RequiredPayloadFieldCoveragePercentagePoints}pp|result {plan.RequiredResultFieldCoveragePercentagePoints}pp|mid {plan.RequiredIntermediateResultFieldCoveragePercentagePoints}pp|raw {plan.RawDataSignalCoveragePercentagePoints}pp|sp {plan.StructuredPayloadSignalCoveragePercentagePoints}pp|sr {plan.StructuredResultSignalCoveragePercentagePoints}pp";
        var intermediate = $"midfields=covered {plan.CoveredRequiredIntermediateResultFieldCount}/{plan.RequiredIntermediateResultFields.Count}:{FormatPreview(plan.CoveredRequiredIntermediateResultFields, 4)}|missing={FormatPreview(plan.MissingRequiredIntermediateResultFields, 4)}";
        var structuredSignals = $"signals=payload {plan.StructuredPayloadSignalCoveredCount}/{plan.RequiredStructuredPayloadSignals.Count}:{FormatPreview(plan.ObservedStructuredPayloadSignals, 4)}|result {plan.StructuredResultSignalCoveredCount}/{plan.RequiredStructuredResultSignals.Count}:{FormatPreview(plan.ObservedStructuredResultSignals, 4)}";
        var evidence = $"evidence=formula {plan.FormulaSignalCoveragePercentagePoints}pp|rules {plan.LegacyAlgorithmRuleCoveragePercentagePoints}pp|decision {plan.LegacyDecisionAnchorCoveragePercentagePoints}pp|decision-rules {plan.LegacyDecisionAnchorObservationRuleCoveragePercentagePoints}pp|decision-resolve {plan.LegacyDecisionAnchorResolutionCoveragePercentagePoints}pp|decision-effective {plan.EffectiveLegacyDecisionAnchorCoveragePercentagePoints}pp";
        var bucketSummary = plan.DependencyBuckets.Count == 0
            ? "buckets=<none>"
            : "buckets=" + string.Join("|", plan.DependencyBuckets.Select(bucket => $"{bucket.BucketKey}:{bucket.CoveredCount}/{bucket.RequiredCount}:{bucket.CoveragePercentagePoints}pp:missing={FormatPreview(bucket.MissingItems, 3)}"));
        var weakestBuckets = plan.DependencyBuckets.Count == 0
            ? "bucket-focus=<none>"
            : "bucket-focus=" + string.Join("|", plan.DependencyBuckets
                .OrderBy(bucket => bucket.CoveragePercentagePoints)
                .ThenByDescending(bucket => bucket.MissingCount)
                .ThenBy(bucket => bucket.BucketKey, StringComparer.Ordinal)
                .Take(3)
                .Select(bucket => $"{bucket.BucketKey}:{bucket.CoveragePercentagePoints}pp:missing={FormatPreview(bucket.MissingItems, 3)}"));
        var nextSteps = $"next={FormatPreview(plan.SuggestedNextSteps, 3)}:{plan.SuggestedNextStepSummary}";
        var anchorNextSteps = $"anchor-next={FormatPreview(plan.SuggestedDecisionAnchorNextSteps, 3)}:{plan.SuggestedDecisionAnchorNextStepSummary}";
        var anchorPriorities = plan.DecisionAnchorPriorityDistributions.Count == 0
            ? "anchor-priority=<none>"
            : "anchor-priority=" + string.Join("|", plan.DecisionAnchorPriorityDistributions.Select(distribution => $"{distribution.Priority}:{distribution.Count}:{distribution.Share:P1}:{FormatPreview(distribution.AnchorKeys, 2)}:{FormatPreview(distribution.SuggestedNextStepFocuses, 2)}:fields={FormatPreview(distribution.SuggestedNextStepFields, 3)}:top={distribution.DominantAnchorKey}:{distribution.DominantSuggestedNextStepFocus}:top-fields={FormatPreview(distribution.DominantSuggestedNextStepFields, 3)}:next={distribution.SuggestedNextStepSummary}"));
        var topAnchorPriority = string.IsNullOrWhiteSpace(plan.DecisionAnchorTopPriority)
            ? "anchor-top-priority=<none>"
            : $"anchor-top-priority={plan.DecisionAnchorTopPriority}:{plan.DecisionAnchorTopPriorityDominantAnchorKey}:{plan.DecisionAnchorTopPriorityFocus}:fields={FormatPreview(plan.DecisionAnchorTopPriorityFields, 3)}:primary={plan.DecisionAnchorTopPriorityPrimaryField}:primary-summary={plan.DecisionAnchorTopPriorityPrimaryFieldSummary}:next={plan.DecisionAnchorTopPriorityNextStepSummary}:summary={plan.DecisionAnchorTopPrioritySummary}";
        var anchorGapPreview = $"anchor-gap={plan.LegacyDecisionAnchorGapPreviewSummary}";
        var anchorPrimary = plan.DecisionAnchorPrimaryFieldDistributions.Count == 0
            ? $"anchor-primary={plan.DecisionAnchorPrimaryFieldSummary}"
            : "anchor-primary=" + string.Join("|", plan.DecisionAnchorPrimaryFieldDistributions.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{string.Join("/", x.AnchorKeys)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.DecisionAnchorPrimaryFieldSummary}";
        var crossPlanAnchorPrimary = plan.CrossPlanDecisionAnchorPrimaryFieldFocuses.Count == 0
            ? $"anchor-cross-plan={plan.CrossPlanDecisionAnchorPrimaryFieldSummary}"
            : "anchor-cross-plan=" + string.Join("|", plan.CrossPlanDecisionAnchorPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:algo-entries={string.Join("/", x.LegacyAlgorithmEntries)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:families={string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.CrossPlanDecisionAnchorPrimaryFieldSummary}";
        var algorithmFamilyAnchorPrimary = plan.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Count == 0
            ? $"anchor-family={plan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary}"
            : "anchor-family=" + string.Join("|", plan.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:families={string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary}";
        var variantKindAnchorPrimary = plan.VariantKindDecisionAnchorPrimaryFieldFocuses.Count == 0
            ? $"anchor-variant={plan.VariantKindDecisionAnchorPrimaryFieldSummary}"
            : "anchor-variant=" + string.Join("|", plan.VariantKindDecisionAnchorPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:variants={string.Join("/", x.VariantKinds)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.VariantKindDecisionAnchorPrimaryFieldSummary}";
        var resultFieldPrimary = plan.RequiredResultPrimaryFieldDistributions.Count == 0
            ? $"result-primary={plan.RequiredResultPrimaryFieldSummary}"
            : "result-primary=" + string.Join("|", plan.RequiredResultPrimaryFieldDistributions.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{string.Join("/", x.BucketKeys)}:{string.Join("/", x.DisplayNames)}")) + $":summary={plan.RequiredResultPrimaryFieldSummary}";
        var crossPlanResultPrimary = plan.CrossPlanRequiredResultPrimaryFieldFocuses.Count == 0
            ? $"result-cross-plan={plan.CrossPlanRequiredResultPrimaryFieldSummary}"
            : "result-cross-plan=" + string.Join("|", plan.CrossPlanRequiredResultPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:families={string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.CrossPlanRequiredResultPrimaryFieldSummary}";
        var algorithmFamilyResultPrimary = plan.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Count == 0
            ? $"result-family={plan.AlgorithmFamilyRequiredResultPrimaryFieldSummary}"
            : "result-family=" + string.Join("|", plan.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:families={string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.AlgorithmFamilyRequiredResultPrimaryFieldSummary}";
        var variantKindResultPrimary = plan.VariantKindRequiredResultPrimaryFieldFocuses.Count == 0
            ? $"result-variant={plan.VariantKindRequiredResultPrimaryFieldSummary}"
            : "result-variant=" + string.Join("|", plan.VariantKindRequiredResultPrimaryFieldFocuses.Take(3).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:weighted={x.WeightedCount}:{x.WeightedShare:P1}:methods={string.Join("/", x.MethodValues)}:method-keys={string.Join("/", x.MethodKeys)}:profiles={string.Join("/", x.ProfileKeys)}:legacy-methods={string.Join("/", x.LegacyMethodNames)}:settings-methods={string.Join("/", x.SettingsMethodNames)}:upstream={string.Join("/", x.UpstreamCanonicalCodes)}:{string.Join("/", x.CanonicalCodes)}:variants={string.Join("/", x.VariantKinds)}:{string.Join("/", x.SuggestedNextStepPriorities)}")) + $":summary={plan.VariantKindRequiredResultPrimaryFieldSummary}";
        var anchorResolutions = plan.LegacyDecisionAnchorResolutions.Count == 0
            ? "anchor-resolutions=<none>"
            : "anchor-resolutions=" + string.Join("|", plan.LegacyDecisionAnchorResolutions.Take(3).Select(FormatDecisionAnchorResolutionPreview));
        var sourceEvidence = plan.SourceEvidences.Count == 0
            ? "source-evidence=<none>"
            : "source-evidence=" + string.Join("|", plan.SourceEvidences.Take(3).Select(x => $"{x.SectionKey}:{x.MethodName}:{x.SourceRange}:fields={FormatPreview(x.ReferencedFields, 4)}:summary={x.Summary}"));
        var formEvidence = plan.FormDependencyEvidences.Count == 0
            ? "form-evidence=<none>"
            : "form-evidence=" + string.Join("|", plan.FormDependencyEvidences.Take(3).Select(x => $"{x.FormName}:{x.SourceRange}:upstream={FormatPreview(x.UpstreamCanonicalCodes, 3)}:methods={FormatPreview(x.ReferencedMethods, 3)}:summary={x.Summary}"));
        var summaries = $"summary=selected={plan.SelectedMethodSummary};compare={plan.BaselineDominantComparisonSummary};decision={plan.LegacyDecisionAnchorResolutionSummary};inputs={plan.LegacyAlgorithmInputReadinessSummary}";

        return $"{plan.CanonicalCode}[{baseline};{dominant};{selected};lead={plan.DominantLeadCount}/{plan.DominantLeadPercentagePoints}pp;algo={plan.AlgorithmEntry};settings={plan.SettingsMethodName};reason={plan.SelectionReason};{readiness};{upstream};{sampleGates};{anchors};{coverage};{intermediate};{structuredSignals};{evidence};{bucketSummary};{weakestBuckets};{nextSteps};{anchorNextSteps};{anchorPriorities};{topAnchorPriority};priority-summary={plan.DecisionAnchorPrioritySummary};{anchorGapPreview};{anchorPrimary};{crossPlanAnchorPrimary};{algorithmFamilyAnchorPrimary};{variantKindAnchorPrimary};{resultFieldPrimary};{crossPlanResultPrimary};{algorithmFamilyResultPrimary};{variantKindResultPrimary};{anchorResolutions};{sourceEvidence};{formEvidence};{summaries};{distributions}]";
    }

    private static string FormatDecisionAnchorResolutionPreview(MotorYDecisionAnchorResolutionSnapshot resolution)
    {
        var status = resolution.ResolvedByObservedPayload
            ? "ok"
            : resolution.PartiallyResolvedByObservedPayload
                ? "partial"
                : "missing";
        var observed = resolution.ObservedPayloadFields.Count == 0
            ? "none"
            : string.Join("+", resolution.ObservedPayloadFields.Take(3));
        var missing = resolution.MissingPayloadFields.Count == 0
            ? "none"
            : string.Join("+", resolution.MissingPayloadFields.Take(3));
        var next = string.IsNullOrWhiteSpace(resolution.SuggestedNextStepSummary)
            ? "none"
            : resolution.SuggestedNextStepSummary;
        var priority = string.IsNullOrWhiteSpace(resolution.SuggestedNextStepPriority)
            ? "none"
            : resolution.SuggestedNextStepPriority;
        var coverage = string.IsNullOrWhiteSpace(resolution.SuggestedNextStepCoverageSummary)
            ? "none"
            : resolution.SuggestedNextStepCoverageSummary;

        var primary = string.IsNullOrWhiteSpace(resolution.SuggestedPrimaryNextField)
            ? "none"
            : resolution.SuggestedPrimaryNextField;
        return $"{resolution.AnchorKey}:{status}:{resolution.CoveragePercentagePoints}pp:obs={observed}:miss={missing}:priority={priority}:primary={primary}:coverage={coverage}:next={next}";
    }

    private static string FormatPreview(IReadOnlyList<string> values, int maxCount)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        var preview = values
            .Take(maxCount)
            .ToArray();
        var suffix = values.Count > maxCount
            ? $"+{values.Count - maxCount}"
            : string.Empty;

        return string.Join("|", preview) + suffix;
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
        var recommended = decision.RecommendedRoute is null
            ? $"recommended={decision.RecommendedStrategy}:<none>"
            : $"recommended={decision.RecommendedStrategy}:{decision.RecommendedRoute.MethodValue}/{decision.RecommendedRoute.ProfileKey}";
        var distributions = decision.Distributions.Count == 0
            ? "dist=<none>"
            : "dist=" + string.Join("|", decision.Distributions.Select(x => $"{x.MethodValue}:{x.Count}:{x.Share:P1}:{x.Route?.VariantKind ?? "unknown"}"));

        return $"{decision.CanonicalCode}[{baseline};{dominant};{recommended};prioritize={(decision.ShouldPrioritizeDominantOverBaseline ? "Y" : "N")};{distributions}]";
    }
}
