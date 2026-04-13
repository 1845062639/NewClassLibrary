using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Infrastructure.Persistence;

namespace StandardTestNext.Test.Application.AppSide;

public static class InProcAppQueryGatewaySeedFactory
{
    public static ITestRecordQueryGateway CreateSeededGateway()
    {
        var recordRepository = new InMemoryTestRecordRepository();
        var attachmentRepository = new InMemoryRecordAttachmentRepository();
        var reportRepository = new InMemoryTestReportRepository();

        Seed(recordRepository, attachmentRepository, reportRepository);

        var queryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
        var facade = new TestRecordQueryFacade(queryService);
        return new TestRecordQueryGatewayAdapter(facade);
    }

    private static void Seed(
        ITestRecordRepository recordRepository,
        IRecordAttachmentRepository attachmentRepository,
        ITestReportRepository reportRepository)
    {
        var seed = TestRecordQuerySeedFactory.CreateDefault();
        var rated = seed.RatedParams;
        var productDefinitionService = new TestProductDefinitionService(new InMemoryProductDefinitionRepository());
        var productDefinition = productDefinitionService.GetOrCreateAsync(rated).GetAwaiter().GetResult();

        var builder = new TestRecordAggregateBuilder();
        var buildResult = builder.BuildDemoRecord(rated, seed.Samples, seed.LegacySamples, productDefinition, seed.NoLoadRConverseType);
        var record = buildResult.Record;

        recordRepository.SaveAsync(record).GetAwaiter().GetResult();
        attachmentRepository.SaveForRecordAsync(record.TestRecordId, record.Attachments).GetAwaiter().GetResult();
        foreach (var item in record.Items)
        {
            attachmentRepository.SaveForRecordItemAsync(item.TestRecordItemId, item.Attachments).GetAwaiter().GetResult();
        }

        var exporter = new TestReportExportService();
        var persistenceService = new TestReportPersistenceService();
        var document = exporter.BuildDocument(record, buildResult.Statistics);
        persistenceService.SaveInlineAsync(document, new JsonTestReportRenderer(), reportRepository, isPrimary: true, isLightweight: false, savedPathPrefix: "inproc://app-query-seed").GetAwaiter().GetResult();
        persistenceService.SaveInlineAsync(document, new MarkdownTestReportRenderer(), reportRepository, isPrimary: false, isLightweight: false, savedPathPrefix: "inproc://app-query-seed").GetAwaiter().GetResult();
        persistenceService.SaveInlineAsync(document, new ManifestTestReportRenderer(), reportRepository, isPrimary: false, isLightweight: true, savedPathPrefix: "inproc://app-query-seed").GetAwaiter().GetResult();
    }

}
