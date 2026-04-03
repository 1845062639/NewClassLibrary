using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYRawDataSignalCoverageSnapshot
{
    public IReadOnlyList<string> RequiredSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingSignals { get; init; } = Array.Empty<string>();
    public int RawSampleCount { get; init; }
    public bool RawDataListAvailable { get; init; }
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYRawDataSignalCoverageEvaluator
{
    private static readonly IReadOnlyDictionary<string, string[]> RequiredSignalsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.LoadA] = new[] { "U", "I1", "P1t", "Nt", "Tt", "Frequency" },
            [MotorYTestMethodCodes.LoadB] = new[] { "U", "I1", "P1t", "Nt", "Tt", "Frequency", "θ1t", "θa" }
        };

    public static MotorYRawDataSignalCoverageSnapshot Evaluate(string canonicalCode, string? sampleDataJson)
    {
        var requiredSignals = RequiredSignalsByCanonicalCode.TryGetValue(canonicalCode, out var configured)
            ? configured.Distinct(StringComparer.Ordinal).ToArray()
            : Array.Empty<string>();

        if (requiredSignals.Length == 0)
        {
            return new MotorYRawDataSignalCoverageSnapshot
            {
                RequiredSignals = Array.Empty<string>(),
                ObservedSignals = Array.Empty<string>(),
                MissingSignals = Array.Empty<string>(),
                RawDataListAvailable = false,
                RawSampleCount = 0,
                CoverageRatio = 1d,
                CoveragePercentagePoints = 100,
                Summary = "raw data signal coverage not required"
            };
        }

        if (string.IsNullOrWhiteSpace(sampleDataJson))
        {
            return Build(requiredSignals, Array.Empty<string>(), rawSampleCount: 0, rawDataListAvailable: false);
        }

        try
        {
            using var document = JsonDocument.Parse(sampleDataJson);
            if (!document.RootElement.TryGetProperty("RawDataList", out var rawDataList)
                || rawDataList.ValueKind != JsonValueKind.Array)
            {
                return Build(requiredSignals, Array.Empty<string>(), rawSampleCount: 0, rawDataListAvailable: false);
            }

            var rawSamples = rawDataList.EnumerateArray().ToArray();
            var observed = requiredSignals
                .Where(signal => rawSamples.Any(sample => HasSignal(sample, signal)))
                .OrderBy(signal => signal, StringComparer.Ordinal)
                .ToArray();
            return Build(requiredSignals, observed, rawSamples.Length, rawDataListAvailable: rawSamples.Length > 0);
        }
        catch (JsonException)
        {
            return Build(requiredSignals, Array.Empty<string>(), rawSampleCount: 0, rawDataListAvailable: false);
        }
    }

    private static MotorYRawDataSignalCoverageSnapshot Build(
        IReadOnlyList<string> requiredSignals,
        IReadOnlyList<string> observedSignals,
        int rawSampleCount,
        bool rawDataListAvailable)
    {
        var missing = requiredSignals
            .Where(signal => !observedSignals.Contains(signal, StringComparer.Ordinal))
            .ToArray();
        var ratio = requiredSignals.Count == 0
            ? 1d
            : Math.Round((double)observedSignals.Count / requiredSignals.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYRawDataSignalCoverageSnapshot
        {
            RequiredSignals = requiredSignals,
            ObservedSignals = observedSignals,
            MissingSignals = missing,
            RawSampleCount = rawSampleCount,
            RawDataListAvailable = rawDataListAvailable,
            CoverageRatio = ratio,
            CoveragePercentagePoints = percentagePoints,
            Summary = $"raw data signals covered {observedSignals.Count}/{requiredSignals.Count} ({percentagePoints}pp); raw samples={rawSampleCount}; missing: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}; observed: {(observedSignals.Count == 0 ? "none" : string.Join(", ", observedSignals))}"
        };
    }

    private static bool HasSignal(JsonElement sample, string signal)
    {
        if (sample.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in sample.EnumerateObject())
        {
            if (string.Equals(property.Name, signal, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind switch
                {
                    JsonValueKind.Number => property.Value.TryGetDouble(out _),
                    JsonValueKind.String => !string.IsNullOrWhiteSpace(property.Value.GetString()),
                    JsonValueKind.True => true,
                    JsonValueKind.False => true,
                    JsonValueKind.Object => true,
                    JsonValueKind.Array => property.Value.GetArrayLength() > 0,
                    _ => false
                };
            }
        }

        return false;
    }
}
