using StandardTestNext.App.Application;
using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application;

var startupOptions = TestStartupOptionsParser.Parse(args);
var messageBusProvider = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS")?.Trim();
var messageBus = MessageBusFactory.Create(new MessageBusOptions
{
    Provider = string.IsNullOrWhiteSpace(messageBusProvider) ? "inmemory" : messageBusProvider
});

var app = new AppBootstrap();
app.Run(messageBus);

var test = new TestBootstrap();
test.Run(messageBus, startupOptions);
