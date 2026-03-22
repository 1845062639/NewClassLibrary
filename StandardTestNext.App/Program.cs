using StandardTestNext.App.Application;
using StandardTestNext.App.Application.Services;
using StandardTestNext.App.ContractsBridge;

var appOptions = AppStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(appOptions.MessageBus);
var validation = RuntimeConfigurationValidator.ValidateApp(appOptions, messageBusOptions);
RuntimeConfigurationConsoleReporter.ReportApp(appOptions, messageBusOptions, validation);
RuntimeConfigurationConsoleReporter.ThrowIfInvalid(validation);

var defaultQueryGateway = InProcAppQueryGatewayFactory.ResolveDefaultGateway();
Console.WriteLine($"[App.Config] queryGateway={(defaultQueryGateway.ResolutionKind == DefaultQueryGatewayResolutionKind.SeededInProc ? "seeded-inproc" : "null-fallback")}");

var messageBus = MessageBusFactory.Create(messageBusOptions);
var app = new AppBootstrap(defaultQueryGateway.Gateway);
app.Run(messageBus, appOptions);
