using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYLockRotorLegacyShape
{
    public double Un { get; init; }
    public double In { get; init; }
    public double Tn { get; init; }
    public int PolePairs { get; init; }
    public double[] CoefficientOfPfe { get; init; } = Array.Empty<double>();
    public double K1 { get; init; }
    public double R1c { get; init; }
    public double θ1c { get; init; }
    public int Order { get; init; }
    public int DecimalPlaces { get; init; }
    public IReadOnlyList<MotorYLockRotorLegacyDataShape> DataList { get; init; } = Array.Empty<MotorYLockRotorLegacyDataShape>();
    public double Ikn { get; init; }
    public double IknDivideIn { get; init; }
    public double Pkn { get; init; }
    public double Tkn { get; init; }
    public double TknDivideTn { get; init; }
    public int TorqueCalType { get; init; }
    public int RCalType { get; init; }
    public double C1 { get; init; }
    public double θw { get; init; }
    public double θb { get; init; }
    public double R1s { get; init; }
    public bool IsAnalysis { get; init; }

    public static MotorYLockRotorLegacyShape? FromJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<MotorYLockRotorLegacyShape>(dataJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
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

public sealed class MotorYLockRotorLegacyDataShape
{
    public double Uk { get; init; }
    public double Ik { get; init; }
    public double Pk { get; init; }
    public double Tk { get; init; }
    public double Pkcu1 { get; init; }
    public double Pfe { get; init; }
    public double ns { get; init; }
    public double θ { get; init; }
    public double R { get; init; }
    public double Frequency { get; init; }
    public double θ1s { get; init; }
    public double UkDivideUn { get; init; }
}
