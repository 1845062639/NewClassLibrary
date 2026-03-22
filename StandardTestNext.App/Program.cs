using StandardTestNext.App.Application;
using StandardTestNext.App.Application.Services;
using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application;
using StandardTestNext.Test.Application.AppSide;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Infrastructure.Persistence;

var appOptions = AppStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(appOptions.MessageBus);
var validation = RuntimeConfigurationValidator.ValidateApp(appOptions, messageBusOptions);
RuntimeConfigurationConsoleReporter.ReportApp(appOptions, messageBusOptions, validation);
RuntimeConfigurationConsoleReporter.ThrowIfInvalid(validation);

var messageBus = MessageBusFactory.Create(messageBusOptions);
var app = new AppBootstrap(BuildDefaultQueryGateway());
app.Run(messageBus, appOptions);

static StandardTestNext.Contracts.ITestRecordQueryGateway BuildDefaultQueryGateway()
{
    var recordRepository = new InMemoryTestRecordRepository();
    var attachmentRepository = new InMemoryRecordAttachmentRepository();
    var reportRepository = new InMemoryTestReportRepository();
    var queryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
    var facade = new TestRecordQueryFacade(queryService);
    return new TestRecordQueryGatewayAdapter(facade);
}
