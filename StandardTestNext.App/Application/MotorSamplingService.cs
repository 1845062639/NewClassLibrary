using StandardTestNext.App.ContractsBridge;
using StandardTestNext.App.Devices;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public sealed class MotorSamplingService
{
    private readonly IMotorDeviceGateway _deviceGateway;
    private readonly IMessagePublisher _publisher;

    public MotorSamplingService(IMotorDeviceGateway deviceGateway, IMessagePublisher publisher)
    {
        _deviceGateway = deviceGateway;
        _publisher = publisher;
    }

    public void PublishSample()
    {
        var sample = _deviceGateway.ReadRealtimeSample();
        _publisher.Publish(ContractTopics.MotorRealtimeSample, sample);
    }
}
