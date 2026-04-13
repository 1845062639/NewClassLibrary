using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYNoLoadExecutionSnapshot
{
    public bool IsExecutable { get; init; }
    public string ExecutionStage { get; init; } = string.Empty;
    public string ExecutionSummary { get; init; } = string.Empty;
    public int RConverseType { get; init; }
    public string RConverseBranch { get; init; } = string.Empty;
    public double ComputedTheta0 { get; init; }
    public double ComputedR0 { get; init; }
    public double RatedVoltagePointRatio { get; init; }
    public double RatedVoltagePointVoltage { get; init; }
    public double RatedVoltagePointCurrent { get; init; }
    public double RatedVoltagePointPower { get; init; }
    public double RatedVoltagePointCopperLoss { get; init; }
    public double RatedVoltagePointConstantLoss { get; init; }
    public double PfwEstimate { get; init; }
    public double PfeEstimate { get; init; }
    public int PfwFitSampleCount { get; init; }
    public bool PfwFitWindowReady { get; init; }
    public IReadOnlyList<string> MissingInputs { get; init; } = Array.Empty<string>();
}

public static class MotorYNoLoadExecutionAdapter
{
    public static MotorYNoLoadExecutionSnapshot Build(string? dataJson)
    {
        var shape = MotorYNoLoadLegacyShape.FromJson(dataJson ?? string.Empty);
        if (shape is null)
        {
            return new MotorYNoLoadExecutionSnapshot
            {
                IsExecutable = false,
                ExecutionStage = "invalid-payload",
                ExecutionSummary = "NoLoad payload 无法解析为 legacy shape",
                MissingInputs = new[] { "DataJson" }
            };
        }

        if (shape.DataList.Count == 0)
        {
            return new MotorYNoLoadExecutionSnapshot
            {
                IsExecutable = false,
                ExecutionStage = "missing-data-list",
                ExecutionSummary = "NoLoad payload 缺少 DataList，无法建立执行入口",
                MissingInputs = new[] { "DataList" }
            };
        }

        var ratedPoint = shape.DataList
            .OrderBy(x => Math.Abs(x.U0DivideUn - 1d))
            .ThenBy(x => Math.Abs(x.U0 - shape.Un))
            .First();

        var computation = MotorYNoLoadComputation.Compute(
            shape.DataList.Select(x => new MotorYNoLoadComputedPoint
            {
                U0 = x.U0,
                U0DivideUn = x.U0DivideUn,
                U0DivideUnSquare = x.U0DivideUnSquare,
                I0 = x.I0,
                P0 = x.P0,
                Theta0 = x.θ0,
                R0 = x.R0,
                DeltaI0 = x.ΔI0,
                P0cu1 = x.P0cu1,
                Pcon = x.Pcon
            }).ToArray(),
            shape.Un,
            shape.Order,
            shape.DecimalPlaces,
            shape.R1c,
            shape.θ1c,
            shape.K1,
            shape.RConverseType,
            shape.R0);
        var pfwEstimate = computation.Pfw;
        var pfeEstimate = computation.Pfe;
        var pfwFitSamples = computation.PfwFitSamples.ToArray();
        var pfwFitWindowReady = computation.PfwFitWindowReady;

        var missingInputs = new List<string>();
        if (shape.Un <= 0)
        {
            missingInputs.Add("Un");
        }

        if (shape.R1c <= 0)
        {
            missingInputs.Add("R1c");
        }

        if (shape.K1 <= 0)
        {
            missingInputs.Add("K1");
        }

        if (shape.DataList.All(x => x.U0 <= 0))
        {
            missingInputs.Add("DataList.U0");
        }

        if (shape.DataList.All(x => x.I0 <= 0))
        {
            missingInputs.Add("DataList.I0");
        }

        if (shape.DataList.All(x => x.P0 <= 0))
        {
            missingInputs.Add("DataList.P0");
        }

        var rConverseBranch = shape.RConverseType == 1 ? "R0->θ0" : "θ0->R0";
        var computedTheta0 = computation.ComputedTheta0;
        var computedR0 = computation.ComputedR0;

        var isExecutable = missingInputs.Count == 0;
        var executionStage = isExecutable
            ? (pfwFitWindowReady ? "rconverse+rated-point+pfw-window" : "rconverse+rated-point-only")
            : "blocked";
        var executionSummary = isExecutable
            ? $"NoLoad 可执行入口已建立：branch={rConverseBranch}, θ0={computedTheta0}, R0={computedR0}, rated-point U0={ratedPoint.U0}, I0={ratedPoint.I0}, P0={ratedPoint.P0}, Pfw={(pfwFitWindowReady ? pfwEstimate : 0)}, Pfe={pfeEstimate}, pfw-fit={(pfwFitWindowReady ? $"ready/{pfwFitSamples.Length}" : $"gap/{pfwFitSamples.Length}")}"
            : $"NoLoad 执行入口阻塞：缺 {string.Join(", ", missingInputs)}";

        return new MotorYNoLoadExecutionSnapshot
        {
            IsExecutable = isExecutable,
            ExecutionStage = executionStage,
            ExecutionSummary = executionSummary,
            RConverseType = shape.RConverseType,
            RConverseBranch = rConverseBranch,
            ComputedTheta0 = computedTheta0,
            ComputedR0 = computedR0,
            RatedVoltagePointRatio = Math.Round(ratedPoint.U0DivideUn, 4),
            RatedVoltagePointVoltage = ratedPoint.U0,
            RatedVoltagePointCurrent = ratedPoint.I0,
            RatedVoltagePointPower = ratedPoint.P0,
            RatedVoltagePointCopperLoss = ratedPoint.P0cu1,
            RatedVoltagePointConstantLoss = ratedPoint.Pcon,
            PfwEstimate = pfwEstimate,
            PfeEstimate = pfeEstimate,
            PfwFitSampleCount = pfwFitSamples.Length,
            PfwFitWindowReady = pfwFitWindowReady,
            MissingInputs = missingInputs
        };
    }
}

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
    public double U0DivideUnIsEquesToOne_Pcu { get; init; }
    public double U0DivideUnIsEquesToOne_Pfe { get; init; }
    public double U0DivideUnIsEquesToOne_DeltaI0 { get; init; }
    public double U0DivideUnIsEquesToOne_R0 { get; init; }
    public double U0DivideUnIsEquesToOne_θ0 { get; init; }
    public double U0DivideUnIsEquesToOne_Theta0 => U0DivideUnIsEquesToOne_θ0 != 0d
        ? U0DivideUnIsEquesToOne_θ0
        : ReadExtraDouble("U0DivideUnIsEquesToOne_Theta0");
    public int PfwFitSampleCount { get; init; }
    public bool PfwFitWindowReady { get; init; }

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

    private double ReadExtraDouble(params string[] fieldNames)
    {
        foreach (var fieldName in fieldNames)
        {
            if (ExtraFields.TryGetValue(fieldName, out var element)
                && element.ValueKind is JsonValueKind.Number
                && element.TryGetDouble(out var value))
            {
                return value;
            }
        }

        return 0d;
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
