using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYThermalLegacyShape
{
    public double Rc { get; init; }
    public double θc { get; init; }
    public double K1 { get; init; }
    public double Rn { get; init; }
    public double θb { get; init; }
    public double Time { get; init; }
    public double θs { get; init; }
    public int Order { get; init; }
    public int DecimalPlaces { get; init; }
    public IReadOnlyList<MotorYThermalLegacyData1Shape> Data1List { get; init; } = Array.Empty<MotorYThermalLegacyData1Shape>();
    public IReadOnlyList<MotorYThermalLegacyData2Shape> Data2List { get; init; } = Array.Empty<MotorYThermalLegacyData2Shape>();
    public double θw { get; init; }
    public double Rws { get; init; }
    public double Rw { get; init; }
    public double Δθ { get; init; }
    public double Δθn { get; init; }
    public double Pn { get; init; }
    public int HotStateType { get; init; }
    public int θbChanelSelect { get; init; }
    public int θ1ChanelSelect { get; init; }
    public bool IsAnalysis { get; init; }
    public bool IsManual { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtraFields { get; init; } = new(StringComparer.Ordinal);

    public double DeltaThetaObserved => Δθ > 0
        ? Δθ
        : ReadExtraDouble("Δθ1") > 0
            ? ReadExtraDouble("Δθ1")
            : ReadExtraDouble("Δθ0");

    public bool HasLegacyDeltaThetaVariant => ExtraFields.ContainsKey("Δθ1") || ExtraFields.ContainsKey("Δθ0");

    public static MotorYThermalLegacyShape? FromJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<MotorYThermalLegacyShape>(dataJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private double ReadExtraDouble(string propertyName)
    {
        return ExtraFields.TryGetValue(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : 0;
    }
}

public sealed class MotorYThermalLegacyData1Shape
{
    public DateTimeOffset Time { get; init; }
    public double U { get; init; }
    public double Iavg { get; init; }
    public double Iu { get; init; }
    public double Iv { get; init; }
    public double Iw { get; init; }
    public double P1 { get; init; }
    public double P2 { get; init; }
    public double Td { get; init; }
    public double N { get; init; }
    public double θb { get; init; }
    public double θ1 { get; init; }
    public double Cosφ { get; init; }
    public double Freq { get; init; }
    public double η { get; init; }
    public double AmbientTemperature { get; init; }
}

public sealed class MotorYThermalLegacyData2Shape
{
    public double Time { get; init; }
    public double R { get; init; }
}
