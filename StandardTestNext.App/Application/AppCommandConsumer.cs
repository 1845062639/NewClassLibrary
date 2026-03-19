using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public sealed class AppCommandConsumer
{
    private readonly IMessageBus _messageBus;
    private readonly DeviceStatusReportingService _statusReportingService;
    private readonly string _deviceId;

    public AppCommandConsumer(IMessageBus messageBus, DeviceStatusReportingService statusReportingService, string deviceId)
    {
        _messageBus = messageBus;
        _statusReportingService = statusReportingService;
        _deviceId = deviceId;
    }

    public void Start()
    {
        _messageBus.Subscribe<TestCommandContract>(ContractTopics.TestCommand, HandleCommand);
    }

    private void HandleCommand(TestCommandContract command)
    {
        Console.WriteLine($"[App] Received command: {command.CommandName} for session {command.SessionId}");
        _statusReportingService.ReportCommandReceived(command, _deviceId);
    }
}
