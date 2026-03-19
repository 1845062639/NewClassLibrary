using StandardTestNext.Contracts;

namespace StandardTestNext.App.Devices;

public interface IMotorDeviceGateway
{
    MotorRealtimeSampleContract ReadRealtimeSample();
}
