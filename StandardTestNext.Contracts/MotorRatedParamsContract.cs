namespace StandardTestNext.Contracts;

public sealed class MotorRatedParamsContract
{
    public string ProductKind { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string StandardCode { get; set; } = string.Empty;
    public double RatedPower { get; set; }
    public double RatedCurrent { get; set; }
    public double RatedVoltage { get; set; }
    public double RatedSpeed { get; set; }
    public double RatedFrequency { get; set; }
    public int Pole { get; set; }
    public int PolePairs { get; set; }
    public string Duty { get; set; } = string.Empty;
    public string DutyRaw { get; set; } = string.Empty;
    public string InsulationGrade { get; set; } = string.Empty;
    public double PowerFactor { get; set; }
    public double Weight { get; set; }
    public string IngressProtection { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
    public string ConnectionRaw { get; set; } = string.Empty;
}
