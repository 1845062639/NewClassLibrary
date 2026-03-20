using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application;
using StandardTestNext.Test.Application.Services;

var testOptions = TestStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(testOptions.MessageBus);
var validation = TestRuntimeConfigurationSupport.ValidateTest(testOptions, messageBusOptions);
TestRuntimeConfigurationSupport.ReportTest(testOptions, messageBusOptions, validation);
TestRuntimeConfigurationSupport.ThrowIfInvalid(validation);

var messageBus = MessageBusFactory.Create(messageBusOptions);
var test = new TestBootstrap();
test.Run(messageBus, testOptions);
