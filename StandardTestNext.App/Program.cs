using StandardTestNext.App.Application;
using StandardTestNext.App.Application.Services;
using StandardTestNext.App.ContractsBridge;

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
    return InProcAppQueryGatewayFactory.CreateSeededGateway();
}
