using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYLoadBLegacyShape
{
    public double Un { get; init; }
    public double Pn { get; init; }
    public double K1 { get; init; }
    public double K2 { get; init; }
    public int PolePairs { get; init; }
    public int Order { get; init; }
    public int DecimalPlaces { get; init; }
    public bool TorqueCorrection { get; init; }
    public double R1c { get; init; }
    public double θ1c { get; init; }
    public double θw { get; init; }
    public double θb { get; init; }
    public double θs { get; init; }
    public double ΔT { get; init; }
    public double A { get; init; }
    public double B { get; init; }
    public double R { get; init; }
    public double Pfw { get; init; }
    public double Pcu1 { get; init; }
    public double Pcu2 { get; init; }
    public double Ps { get; init; }
    public double[] CoefficientOfPfe { get; init; } = Array.Empty<double>();
    public IReadOnlyList<MotorYLoadBRawDataShape> RawDataList { get; init; } = Array.Empty<MotorYLoadBRawDataShape>();
    public IReadOnlyList<MotorYLoadBResultDataShape> ResultDataList { get; init; } = Array.Empty<MotorYLoadBResultDataShape>();
    public bool IsAnalysis { get; init; }
    public bool IsTorqueModify { get; init; }
    public int θ1tChanelSelect { get; init; }
    public int θaChanelSelect { get; init; }

    public static MotorYLoadBLegacyShape? FromJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<MotorYLoadBLegacyShape>(dataJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public sealed class MotorYLoadBRawDataShape
{
    public double U { get; init; }
    public double I1 { get; init; }
    public double Nt { get; init; }
    public double P1t { get; init; }
    public double Frequency { get; init; }
    public double θ1t { get; init; }
    public double θa { get; init; }
    public double Tt { get; init; }
    public double P2x { get; init; }
    public double η { get; init; }
    public double Cosφ { get; init; }
    public double AmbientTemperature { get; init; }
}

public sealed class MotorYLoadBResultDataShape
{
    public double P1 { get; init; }
    public double P2 { get; init; }
    public double I1 { get; init; }
    public double η { get; init; }
    public double Cosφ { get; init; }
    public double S { get; init; }
    public double Percentage { get; init; }
    public double Pcu1x { get; init; }
    public double Pcu2x { get; init; }
    public double Ps { get; init; }
}
