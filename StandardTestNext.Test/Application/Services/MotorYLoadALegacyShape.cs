using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYLoadALegacyShape
{
    public double Un { get; init; }
    public double Pn { get; init; }
    public double R1c { get; init; }
    public double θ1c { get; init; }
    public double K1 { get; init; }
    public double K2 { get; init; }
    public double θa { get; init; }
    public double ΔT { get; init; }
    public double Pfw { get; init; }
    public int Order { get; init; }
    public int PolePairs { get; init; }
    public int DecimalPlaces { get; init; }
    public double[] CoefficientOfPfe { get; init; } = Array.Empty<double>();
    public IReadOnlyList<MotorYLoadARawDataShape> RawDataList { get; init; } = Array.Empty<MotorYLoadARawDataShape>();
    public IReadOnlyList<MotorYLoadAResultDataShape> ResultDataList { get; init; } = Array.Empty<MotorYLoadAResultDataShape>();
    public int CorrectionType { get; init; }
    public bool IsAnalysis { get; init; }

    public static MotorYLoadALegacyShape? FromJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<MotorYLoadALegacyShape>(dataJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
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

public sealed class MotorYLoadARawDataShape
{
    public double U { get; init; }
    public double I1 { get; init; }
    public double Nt { get; init; }
    public double P1t { get; init; }
    public double Frequency { get; init; }
    public double Tt { get; init; }
    public double P2x { get; init; }
    public double η { get; init; }
    public double Cosφ { get; init; }
}

public sealed class MotorYLoadAResultDataShape
{
    public double P1 { get; init; }
    public double P2 { get; init; }
    public double I1 { get; init; }
    public double η { get; init; }
    public double Cosφ { get; init; }
    public double S { get; init; }
    public double Percentage { get; init; }
}
