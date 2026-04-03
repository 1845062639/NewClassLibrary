using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYRequiredResultFieldCoverageEvaluator
{
    public static MotorYRequiredResultFieldCoverageSnapshot Evaluate(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        string? sampleDataJson)
    {
        if (requiredFields.Count == 0)
        {
            return new MotorYRequiredResultFieldCoverageSnapshot
            {
                CanonicalCode = canonicalCode,
                CoveredRequiredResultFieldCount = 0,
                MissingRequiredResultFieldCount = 0,
                MissingRequiredResultFields = Array.Empty<string>(),
                CoveredRequiredResultFields = Array.Empty<string>(),
                RequiredResultFieldCoverageRatio = 1d,
                RequiredResultFieldCoveragePercentagePoints = 100,
                SamplePayloadAvailable = !string.IsNullOrWhiteSpace(sampleDataJson),
                RequiredResultFieldCoverageSummary = "no required result fields"
            };
        }

        if (string.IsNullOrWhiteSpace(sampleDataJson))
        {
            return Build(canonicalCode, requiredFields, Array.Empty<string>(), samplePayloadAvailable: false);
        }

        try
        {
            using var document = JsonDocument.Parse(sampleDataJson);
            var coveredFields = requiredFields
                .Where(field => HasField(document.RootElement, field))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return Build(canonicalCode, requiredFields, coveredFields, samplePayloadAvailable: true);
        }
        catch (JsonException)
        {
            return Build(canonicalCode, requiredFields, Array.Empty<string>(), samplePayloadAvailable: true);
        }
    }

    private static MotorYRequiredResultFieldCoverageSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> coveredFields,
        bool samplePayloadAvailable)
    {
        var missingFields = requiredFields
            .Where(field => !coveredFields.Contains(field, StringComparer.Ordinal))
            .ToArray();
        var coveredRequiredResultFields = coveredFields
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var ratio = requiredFields.Count == 0
            ? 1d
            : Math.Round((double)coveredRequiredResultFields.Length / requiredFields.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYRequiredResultFieldCoverageSnapshot
        {
            CanonicalCode = canonicalCode,
            CoveredRequiredResultFieldCount = coveredRequiredResultFields.Length,
            MissingRequiredResultFieldCount = missingFields.Length,
            MissingRequiredResultFields = missingFields,
            CoveredRequiredResultFields = coveredRequiredResultFields,
            RequiredResultFieldCoverageRatio = ratio,
            RequiredResultFieldCoveragePercentagePoints = percentagePoints,
            SamplePayloadAvailable = samplePayloadAvailable,
            RequiredResultFieldCoverageSummary = $"result required fields covered {coveredRequiredResultFields.Length}/{requiredFields.Count} ({percentagePoints}pp); missing: {(missingFields.Length == 0 ? "none" : string.Join(", ", missingFields))}"
        };
    }

    private static bool HasField(JsonElement element, string fieldName)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => HasFieldInObject(element, fieldName),
            JsonValueKind.Array => element.EnumerateArray().Any(item => HasField(item, fieldName)),
            _ => false
        };
    }

    private static bool HasFieldInObject(JsonElement element, string fieldName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, fieldName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (HasField(property.Value, fieldName))
            {
                return true;
            }
        }

        return false;
    }
}

internal sealed class MotorYRequiredResultFieldCoverageSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int CoveredRequiredResultFieldCount { get; init; }
    public int MissingRequiredResultFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredResultFields { get; init; } = Array.Empty<string>();
    public double RequiredResultFieldCoverageRatio { get; init; }
    public int RequiredResultFieldCoveragePercentagePoints { get; init; }
    public bool SamplePayloadAvailable { get; init; }
    public string RequiredResultFieldCoverageSummary { get; init; } = string.Empty;
}
