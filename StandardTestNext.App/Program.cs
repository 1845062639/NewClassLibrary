using StandardTestNext.App.Application;
using StandardTestNext.App.Application.Services;
using StandardTestNext.App.ContractsBridge;

var runSmokeTests = args.Any(arg => string.Equals(arg, "--smoke", StringComparison.OrdinalIgnoreCase)
    || string.Equals(arg, "--run-smoke-tests", StringComparison.OrdinalIgnoreCase));

if (runSmokeTests)
{
    Console.WriteLine("[App] Running smoke tests...");
    AppQueryGatewaySmokeTests.Run();
    Console.WriteLine("[App] Smoke tests passed.");
    return;
}

var appOptions = AppStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(appOptions.MessageBus);
var validation = RuntimeConfigurationValidator.ValidateApp(appOptions, messageBusOptions);
RuntimeConfigurationConsoleReporter.ReportApp(appOptions, messageBusOptions, validation);
RuntimeConfigurationConsoleReporter.ThrowIfInvalid(validation);

var defaultQueryGateway = InProcAppQueryGatewayFactory.ResolveDefaultGateway(appOptions.QueryGatewayMode);
Console.WriteLine($"[App.Config] queryGateway.requested={appOptions.QueryGatewayMode}");
Console.WriteLine($"[App.Config] queryGateway.resolved={(defaultQueryGateway.ResolutionKind == DefaultQueryGatewayResolutionKind.SeededInProc ? "seeded-inproc" : "null-fallback")}");

var messageBus = MessageBusFactory.Create(messageBusOptions);
var app = new AppBootstrap(defaultQueryGateway.Gateway);
app.Run(messageBus, appOptions);
