using StandardTestNext.App.Application;
using StandardTestNext.App.Application.Services;
using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application;
using StandardTestNext.Test.Application.Services;

var appConfiguration = AppRuntimeConfigurationLoader.Load();
var messageBusProvider = Environment.GetEnvironmentVariable("STNEXT_MESSAGE_BUS")?.Trim();
if (string.IsNullOrWhiteSpace(messageBusProvider))
{
    messageBusProvider = appConfiguration.MessageBusProvider;
}

var messageBus = MessageBusFactory.Create(new MessageBusOptions
{
    Provider = messageBusProvider,
    Host = appConfiguration.MessageBus.Host,
    Port = appConfiguration.MessageBus.Port,
    ClientId = appConfiguration.MessageBus.ClientId,
    TopicPrefix = appConfiguration.MessageBus.TopicPrefix,
    Username = appConfiguration.MessageBus.Username,
    Password = appConfiguration.MessageBus.Password
});

var appOptions = AppStartupOptionsParser.Parse(args);
var testOptions = TestStartupOptionsParser.Parse(args);

var app = new AppBootstrap();
app.Run(messageBus, appOptions);

var test = new TestBootstrap();
test.Run(messageBus, testOptions);
