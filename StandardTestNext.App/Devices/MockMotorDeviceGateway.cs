using StandardTestNext.Contracts;

namespace StandardTestNext.App.Devices;

public sealed class MockMotorDeviceGateway : IMotorDeviceGateway
{
    public MotorRealtimeSampleContract ReadRealtimeSample()
    {
        return new MotorRealtimeSampleContract
        {
            SampleTime = DateTimeOffset.Now,
            DeviceId = "mock-motor-device",
            ProductKind = "Motor_Y",
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
