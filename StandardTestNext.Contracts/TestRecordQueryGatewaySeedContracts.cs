namespace StandardTestNext.Contracts;

public sealed class TestRecordQuerySeedContract
{
    public MotorRatedParamsContract RatedParams { get; init; } = new();
    public IReadOnlyList<MotorRealtimeSampleContract> Samples { get; init; } = Array.Empty<MotorRealtimeSampleContract>();
}

public static class TestRecordQuerySeedFactory
{
    public static TestRecordQuerySeedContract CreateDefault()
    {
        var now = DateTimeOffset.Now;
        var rated = new MotorRatedParamsContract
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

        return new TestRecordQuerySeedContract
        {
            RatedParams = rated,
            Samples =
            [
                new MotorRealtimeSampleContract
                {
                    SampleTime = now,
                    DeviceId = "app-query-seed-01",
                    ProductKind = rated.ProductKind,
                    VoltageAverage = 380.1,
                    CurrentAverage = 21.6,
                    Power = 10.4,
                    Frequency = 50,
                    Speed = 1472,
                    Torque = 68.1,
                    IsRecordPoint = true
                },
                new MotorRealtimeSampleContract
                {
                    SampleTime = now.AddSeconds(1),
                    DeviceId = "app-query-seed-01",
                    ProductKind = rated.ProductKind,
                    VoltageAverage = 379.8,
                    CurrentAverage = 21.8,
                    Power = 10.5,
                    Frequency = 50,
                    Speed = 1471,
                    Torque = 68.7,
                    IsRecordPoint = false
                },
                new MotorRealtimeSampleContract
                {
                    SampleTime = now.AddSeconds(2),
                    DeviceId = "app-query-seed-01",
                    ProductKind = rated.ProductKind,
                    VoltageAverage = 380.4,
                    CurrentAverage = 21.7,
                    Power = 10.6,
                    Frequency = 50,
                    Speed = 1473,
                    Torque = 68.5,
                    IsRecordPoint = true
                }
            ]
        };
    }
}
