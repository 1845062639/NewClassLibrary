using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYStructuredSignalCoverageSnapshot
{
    public IReadOnlyList<string> RequiredSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingSignals { get; init; } = Array.Empty<string>();
    public int CoveredSignalCount { get; init; }
    public int MissingSignalCount { get; init; }
    public int SampleCount { get; init; }
    public bool StructuredDataAvailable { get; init; }
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYStructuredSignalCoverageEvaluator
{
    public static MotorYStructuredSignalCoverageSnapshot Evaluate(IReadOnlyList<string>? requiredSignals, string? sampleDataJson, string summaryLabel)
    {
        var required = (requiredSignals ?? Array.Empty<string>())
            .Where(signal => !string.IsNullOrWhiteSpace(signal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (required.Length == 0)
        {
            return new MotorYStructuredSignalCoverageSnapshot
            {
                RequiredSignals = Array.Empty<string>(),
                ObservedSignals = Array.Empty<string>(),
                MissingSignals = Array.Empty<string>(),
                CoveredSignalCount = 0,
                MissingSignalCount = 0,
                SampleCount = 0,
                StructuredDataAvailable = false,
                CoverageRatio = 1d,
                CoveragePercentagePoints = 100,
                Summary = $"{summaryLabel} not required"
            };
        }

        if (string.IsNullOrWhiteSpace(sampleDataJson))
        {
            return Build(required, Array.Empty<string>(), 0, false, summaryLabel);
        }

        try
        {
            using var document = JsonDocument.Parse(sampleDataJson);
            var sampleCount = 0;
            var observed = required
                .Where(signal => HasSignal(document.RootElement, signal, ref sampleCount))
                .OrderBy(signal => signal, StringComparer.Ordinal)
                .ToArray();
            return Build(required, observed, sampleCount, observed.Length > 0 || sampleCount > 0, summaryLabel);
        }
        catch (JsonException)
        {
            return Build(required, Array.Empty<string>(), 0, false, summaryLabel);
        }
    }

    private static MotorYStructuredSignalCoverageSnapshot Build(
        IReadOnlyList<string> requiredSignals,
        IReadOnlyList<string> observedSignals,
        int sampleCount,
        bool structuredDataAvailable,
        string summaryLabel)
    {
        var missing = requiredSignals
            .Where(signal => !observedSignals.Contains(signal, StringComparer.Ordinal))
            .ToArray();
        var ratio = requiredSignals.Count == 0
            ? 1d
            : Math.Round((double)observedSignals.Count / requiredSignals.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYStructuredSignalCoverageSnapshot
        {
            RequiredSignals = requiredSignals,
            ObservedSignals = observedSignals,
            MissingSignals = missing,
            CoveredSignalCount = observedSignals.Count,
            MissingSignalCount = missing.Length,
            SampleCount = sampleCount,
            StructuredDataAvailable = structuredDataAvailable,
            CoverageRatio = ratio,
            CoveragePercentagePoints = percentagePoints,
            Summary = $"{summaryLabel} covered {observedSignals.Count}/{requiredSignals.Count} ({percentagePoints}pp); samples={sampleCount}; missing: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}; observed: {(observedSignals.Count == 0 ? "none" : string.Join(", ", observedSignals))}"
        };
    }

    private static bool HasSignal(JsonElement root, string signalPath, ref int sampleCount)
    {
        var segments = signalPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        JsonElement current = root;
        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            var isLeaf = index == segments.Length - 1;

            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!TryGetPropertyIgnoreCase(current, segment, out current))
                {
                    return false;
                }

                if (isLeaf)
                {
                    return HasValue(current);
                }

                continue;
            }

            if (current.ValueKind == JsonValueKind.Array)
            {
                var items = current.EnumerateArray().ToArray();
                sampleCount = Math.Max(sampleCount, items.Length);
                var remainingPath = string.Join('.', segments.Skip(index));
                foreach (var item in items)
                {
                    if (HasSignal(item, remainingPath, ref sampleCount))
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        return false;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool HasValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetDouble(out _),
            JsonValueKind.String => !string.IsNullOrWhiteSpace(value.GetString()),
            JsonValueKind.True => true,
            JsonValueKind.False => true,
            JsonValueKind.Object => value.EnumerateObject().Any(),
            JsonValueKind.Array => value.GetArrayLength() > 0,
            _ => false
        };
    }
}
