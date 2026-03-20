using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application;
using StandardTestNext.Test.Application.Services;

var testOptions = TestStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(testOptions.MessageBus);
var validation = RuntimeConfigurationValidator.ValidateTest(testOptions, messageBusOptions);
RuntimeConfigurationConsoleReporter.ReportTest(testOptions, messageBusOptions, validation);

var messageBus = MessageBusFactory.Create(messageBusOptions);
var test = new TestBootstrap();
test.Run(messageBus, testOptions);
