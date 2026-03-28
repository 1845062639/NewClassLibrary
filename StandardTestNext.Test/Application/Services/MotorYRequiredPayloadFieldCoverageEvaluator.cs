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
                RequiredPayloadFieldCoverageSummary = "no required payload fields"
            };
        }

        if (string.IsNullOrWhiteSpace(sampleDataJson))
        {
            return Build(canonicalCode, requiredFields, Array.Empty<string>());
        }

        try
        {
            using var document = JsonDocument.Parse(sampleDataJson);
            var coveredFields = requiredFields
                .Where(field => HasField(document.RootElement, field))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return Build(canonicalCode, requiredFields, coveredFields);
        }
        catch (JsonException)
        {
            return Build(canonicalCode, requiredFields, Array.Empty<string>());
        }
    }

    private static MotorYRequiredPayloadFieldCoverageSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> coveredFields)
    {
        var missingFields = requiredFields
            .Where(field => !coveredFields.Contains(field, StringComparer.Ordinal))
            .ToArray();

        return new MotorYRequiredPayloadFieldCoverageSnapshot
        {
            CanonicalCode = canonicalCode,
            CoveredRequiredPayloadFieldCount = coveredFields.Count,
            MissingRequiredPayloadFieldCount = missingFields.Length,
            MissingRequiredPayloadFields = missingFields,
            RequiredPayloadFieldCoverageSummary = $"payload required fields covered {coveredFields.Count}/{requiredFields.Count}; missing: {(missingFields.Length == 0 ? "none" : string.Join(", ", missingFields))}"
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
    public string RequiredPayloadFieldCoverageSummary { get; init; } = string.Empty;
}
