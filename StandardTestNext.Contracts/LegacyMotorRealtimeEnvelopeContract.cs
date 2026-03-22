using System.Text.Json.Serialization;

namespace StandardTestNext.Contracts;

public sealed class LegacyMotorRealtimeEnvelopeContract
{
    public DateTimeOffset SampleTime { get; init; }
    public string DeviceId { get; init; } = string.Empty;
    public string ProductKind { get; init; } = string.Empty;
    public double VoltageAverage { get; init; }
    public double CurrentAverage { get; init; }
    public double Power { get; init; }
    public double Frequency { get; init; }
    public double Speed { get; init; }
    public double Torque { get; init; }
    public bool IsRecordPoint { get; init; }

    // Legacy/raw payload fields observed in historical StandardTestApp projects.
    public double? Uab { get; init; }
    public double? Ubc { get; init; }
    public double? Uca { get; init; }
    public double? Uavg { get; init; }
    public double? Ia { get; init; }
    public double? Ib { get; init; }
    public double? Ic { get; init; }
    public double? Iavg { get; init; }
    public double? CosPhi { get; init; }
    public double? DeltaI { get; init; }
    public double? AccX1 { get; init; }
    public double? AccY1 { get; init; }
    public double? AccZ1 { get; init; }
    public double? VelX1 { get; init; }
    public double? VelY1 { get; init; }
    public double? VelZ1 { get; init; }
    public double? DisX1 { get; init; }
    public double? DisY1 { get; init; }
    public double? DisZ1 { get; init; }
    public double? AccX2 { get; init; }
    public double? AccY2 { get; init; }
    public double? AccZ2 { get; init; }
    public double? VelX2 { get; init; }
    public double? VelY2 { get; init; }
    public double? VelZ2 { get; init; }
    public double? DisX2 { get; init; }
    public double? DisY2 { get; init; }
    public double? DisZ2 { get; init; }
    public double? Temp1 { get; init; }
    public double? Temp2 { get; init; }
    public double? Temp3 { get; init; }
    public double? Temp4 { get; init; }
    public double? Temp5 { get; init; }
    public double? Temp6 { get; init; }
    public double? Temp7 { get; init; }
    public double? Temp8 { get; init; }
    public double? AmbientHumidity { get; init; }
    public double? AmbientTemperature { get; init; }
    public double? UabIncoming { get; init; }
    public double? UbcIncoming { get; init; }
    public double? UcaIncoming { get; init; }
    public double? UavgIncoming { get; init; }
    public double? IaIncoming { get; init; }
    public double? IbIncoming { get; init; }
    public double? IcIncoming { get; init; }
    public double? IavgIncoming { get; init; }
    public double? PIncoming { get; init; }
    public double? FrequencyIncoming { get; init; }
    public string? LeaveFactoryModePowerCurveImage { get; init; }
    public string? LeaveFactoryModeTempCurveImage { get; init; }
    public string? LeaveFactoryModeVibrationCurveImage { get; init; }
    public string? LeaveFactoryModeVibrationFrequencyCurveImage { get; init; }
    public string? TempRiseModePowerCurveImage { get; init; }
    public string? TempRiseModeTempCurveImage { get; init; }
    public string? TempRiseModeVibrationCurveImage { get; init; }
    public string? TempRiseModeVibrationFrequencyCurveImage { get; init; }

    [JsonIgnore]
    public MotorRealtimeSampleContract CoreSample => new()
    {
        SampleTime = SampleTime,
        DeviceId = DeviceId,
        ProductKind = ProductKind,
        VoltageAverage = VoltageAverage,
        CurrentAverage = CurrentAverage,
        Power = Power,
        Frequency = Frequency,
        Speed = Speed,
        Torque = Torque,
        IsRecordPoint = IsRecordPoint
    };
}
