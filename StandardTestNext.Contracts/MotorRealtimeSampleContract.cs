namespace StandardTestNext.Contracts;

public sealed class MotorRealtimeSampleContract
{
    public DateTimeOffset SampleTime { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public double VoltageAverage { get; set; }
    public double CurrentAverage { get; set; }
    public double Power { get; set; }
    public double Frequency { get; set; }
    public double Speed { get; set; }
    public double Torque { get; set; }
    public bool IsRecordPoint { get; set; }
}
