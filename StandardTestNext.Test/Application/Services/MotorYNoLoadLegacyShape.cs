using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYNoLoadLegacyShape
{
    public double Un { get; init; }
    public double R1c { get; init; }
    public double θ1c { get; init; }
    public double K1 { get; init; }
    public int Order { get; init; }
    public int DecimalPlaces { get; init; }
    public IReadOnlyList<MotorYNoLoadLegacyDataShape> DataList { get; init; } = Array.Empty<MotorYNoLoadLegacyDataShape>();
    public double P0 { get; init; }
    public double I0 { get; init; }
    public double ΔI0 { get; init; }
    public double Pcu { get; init; }
    public double Pfw { get; init; }
    public double Pfe { get; init; }
    public double θ0 { get; init; }
    public double R0 { get; init; }
    public double[] CoefficientOfPfe { get; init; } = Array.Empty<double>();
    public int RConverseType { get; init; }
    public bool IsAnalysis { get; init; }
    public double U0DivideUnIsEquesToOne_I0 { get; init; }
    public double U0DivideUnIsEquesToOne_P0 { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtraFields { get; init; } = new(StringComparer.Ordinal);

    public bool HasSinglePointSnapshot => ExtraFields.ContainsKey("U0") || ExtraFields.ContainsKey("P0cu1") || ExtraFields.ContainsKey("Frequency");

    public static MotorYNoLoadLegacyShape? FromJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MotorYNoLoadLegacyShape>(dataJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
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

public sealed class MotorYNoLoadLegacyDataShape
{
    public double U0 { get; init; }
    public double U0DivideUn { get; init; }
    public double U0DivideUnSquare { get; init; }
    public double I0 { get; init; }
    public double I01 { get; init; }
    public double I02 { get; init; }
    public double I03 { get; init; }
    public double P0 { get; init; }
    public double Cosφ { get; init; }
    public double Frequency { get; init; }
    public double θ0 { get; init; }
    public double R0 { get; init; }
    public double ΔI0 { get; init; }
    public double P0cu1 { get; init; }
    public double Pcon { get; init; }
    public double Pfe { get; init; }
    public double n0 { get; init; }
    public double T0 { get; init; }
}
