using StandardTestNext.App.ContractsBridge;
using StandardTestNext.App.Devices;

namespace StandardTestNext.App.Application;

public sealed class AppBootstrap
{
    public void Run()
    {
        Console.WriteLine("StandardTestNext.App starting...");

        var messageBus = new InMemoryMessageBus();
        var deviceGateway = new MockMotorDeviceGateway();
        var sampleService = new MotorSamplingService(deviceGateway, messageBus);

        sampleService.PublishSample();

        Console.WriteLine("StandardTestNext.App ready.");
    }
}
