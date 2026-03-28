using StandardTestNext.Contracts;
using StandardTestNext.Test.Application;
using StandardTestNext.Test.RuntimeBridge;
using StandardTestNext.Test.Application.RuntimeBridge;
using StandardTestNext.Test.Application.Services;

var testOptions = TestStartupOptionsParser.Parse(args);
var messageBusOptions = MessageBusOptionsFactory.Create(testOptions.MessageBus);
var validation = TestRuntimeConfigurationSupport.ValidateTest(testOptions, messageBusOptions);
TestRuntimeConfigurationSupport.ReportTest(testOptions, messageBusOptions, validation);
TestRuntimeConfigurationSupport.ThrowIfInvalid(validation);

if (testOptions.RunSmokeTests)
{
    Console.WriteLine("[Test] Running smoke tests...");
    TestRecordLegacyPayloadReaderSmokeTests.Run();
    TestRecordQueryGatewayAdapterSmokeTests.Run();
    StpDbMotorYPayloadSmokeTests.Run();
    MotorYStpDbShapeAlignmentSmokeTests.Run();
    Console.WriteLine("[Test] Smoke tests passed.");
    return;
}

var messageBus = MessageBusFactory.Create(messageBusOptions);
var test = new TestBootstrap();
test.Run(messageBus, testOptions);
