using StandardTestNext.Contracts;

namespace StandardTestNext.App.Devices;

public sealed class MockMotorDeviceGateway : IMotorDeviceGateway
{
    private readonly string _deviceId;
    private readonly string _productKind;

    public MockMotorDeviceGateway(string deviceId, string productKind)
    {
        _deviceId = deviceId;
        _productKind = productKind;
    }

    public MotorRealtimeSampleContract ReadRealtimeSample()
    {
        return new MotorRealtimeSampleContract
        {
            SampleTime = DateTimeOffset.Now,
            DeviceId = _deviceId,
            ProductKind = _productKind,
            VoltageAverage = 380,
            CurrentAverage = 12.5,
            Power = 6.8,
            Frequency = 50,
            Speed = 1480,
            Torque = 43.2,
            IsRecordPoint = true
        };
    }
}
