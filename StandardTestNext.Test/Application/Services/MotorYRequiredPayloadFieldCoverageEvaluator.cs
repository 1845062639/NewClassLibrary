using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYRequiredPayloadFieldCoverageEvaluator
{
    public static MotorYRequiredPayloadFieldCoverageSnapshot Evaluate(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        string? sampleDataJson)
    {
        if (requiredFields.Count == 0)
        {
            return new MotorYRequiredPayloadFieldCoverageSnapshot
            {
                CanonicalCode = canonicalCode,
                CoveredRequiredPayloadFieldCount = 0,
                MissingRequiredPayloadFieldCount = 0,
                MissingRequiredPayloadFields = Array.Empty<string>(),
                RequiredPayloadFieldCoverageSummary = "no required payload fields",
                CoveredRequiredPayloadFields = Array.Empty<string>(),
                RequiredPayloadFieldCoverageRatio = 1d,
                RequiredPayloadFieldCoveragePercentagePoints = 100,
                SamplePayloadAvailable = !string.IsNullOrWhiteSpace(sampleDataJson)
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

    private static MotorYRequiredPayloadFieldCoverageSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> coveredFields,
        bool samplePayloadAvailable)
    {
        var missingFields = requiredFields
            .Where(field => !coveredFields.Contains(field, StringComparer.Ordinal))
            .ToArray();
        var coveredRequiredPayloadFields = coveredFields
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var ratio = requiredFields.Count == 0
            ? 1d
            : Math.Round((double)coveredRequiredPayloadFields.Length / requiredFields.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYRequiredPayloadFieldCoverageSnapshot
        {
            CanonicalCode = canonicalCode,
            CoveredRequiredPayloadFieldCount = coveredRequiredPayloadFields.Length,
            MissingRequiredPayloadFieldCount = missingFields.Length,
            MissingRequiredPayloadFields = missingFields,
            CoveredRequiredPayloadFields = coveredRequiredPayloadFields,
            RequiredPayloadFieldCoverageRatio = ratio,
            RequiredPayloadFieldCoveragePercentagePoints = percentagePoints,
            SamplePayloadAvailable = samplePayloadAvailable,
            RequiredPayloadFieldCoverageSummary = $"payload required fields covered {coveredRequiredPayloadFields.Length}/{requiredFields.Count} ({percentagePoints}pp); missing: {(missingFields.Length == 0 ? "none" : string.Join(", ", missingFields))}"
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

internal sealed class MotorYRequiredPayloadFieldCoverageSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int CoveredRequiredPayloadFieldCount { get; init; }
    public int MissingRequiredPayloadFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredPayloadFields { get; init; } = Array.Empty<string>();
    public double RequiredPayloadFieldCoverageRatio { get; init; }
    public int RequiredPayloadFieldCoveragePercentagePoints { get; init; }
    public bool SamplePayloadAvailable { get; init; }
    public string RequiredPayloadFieldCoverageSummary { get; init; } = string.Empty;
}
