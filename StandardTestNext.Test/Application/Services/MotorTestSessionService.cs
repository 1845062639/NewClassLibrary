using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorTestSessionService
{
    public MotorRatedParamsContract BuildDemoRatedParams()
    {
        return new MotorRatedParamsContract
        {
            ProductKind = "Motor_Y",
            Model = "Y-160M-4",
            StandardCode = "GB1032-2023",
            RatedPower = 11,
            RatedCurrent = 22.4,
            RatedVoltage = 380,
            RatedSpeed = 1470,
            RatedFrequency = 50,
            Pole = 4,
            PolePairs = 2,
            Duty = "S1",
            InsulationGrade = "F",
            PowerFactor = 0.86,
            Weight = 95,
            IngressProtection = "IP55",
            Connection = "Delta"
        };
    }

    public void PrintReadyState()
    {
        var rated = BuildDemoRatedParams();
        Console.WriteLine($"[Test] Ready rated params for {rated.Model}");
    }
}
