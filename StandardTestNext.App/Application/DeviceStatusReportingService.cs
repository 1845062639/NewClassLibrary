using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public sealed class DeviceStatusReportingService
{
    private readonly IMessagePublisher _publisher;

    public DeviceStatusReportingService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public void ReportReady(string deviceId, string productKind)
    {
        var status = new DeviceStatusContract
        {
            DeviceId = deviceId,
            ProductKind = productKind,
            ConnectionState = "Connected",
            WorkState = "Ready",
            StatusTime = DateTimeOffset.Now
        };

        _publisher.Publish(ContractTopics.DeviceStatus, status);
    }

    public void ReportCommandReceived(TestCommandContract command, string deviceId)
    {
        var status = new DeviceStatusContract
        {
            DeviceId = deviceId,
            ProductKind = command.ProductKind,
            ConnectionState = "Connected",
            WorkState = "Running",
            ActiveCommandName = command.CommandName,
            StatusTime = DateTimeOffset.Now
        };

        _publisher.Publish(ContractTopics.DeviceStatus, status);
    }
}
