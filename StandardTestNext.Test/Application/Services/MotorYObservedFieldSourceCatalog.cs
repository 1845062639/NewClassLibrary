namespace StandardTestNext.Test.Application.Services;

internal static class MotorYObservedFieldSourceCatalog
{
    public static IReadOnlyList<MotorYObservedFieldSourceSnapshot> Build(
        IReadOnlyList<string>? payloadFields,
        IReadOnlyList<string>? ratedParamFields,
        IReadOnlyList<string>? resultFields,
        IReadOnlyList<string>? intermediateResultFields,
        IReadOnlyList<string>? rawDataSignals,
        IReadOnlyList<string>? structuredPayloadSignals,
        IReadOnlyList<string>? structuredResultSignals)
    {
        var items = new List<MotorYObservedFieldSourceSnapshot>();

        Add(items, payloadFields, "payload-field", "payload", "observed directly in sample payload");
        Add(items, ratedParamFields, "rated-param-field", "rated-params", "observed from normalized rated params");
        Add(items, resultFields, "result-field", "result", "observed from result payload/result block");
        Add(items, intermediateResultFields, "intermediate-result-field", "intermediate-result", "observed from legacy intermediate result payload/result block");
        Add(items, rawDataSignals, "raw-data-signal", "raw-data", "observed from raw sample list / raw data signals");
        Add(items, structuredPayloadSignals, "structured-payload-signal", "structured-payload", "observed from structured payload list");
        Add(items, structuredResultSignals, "structured-result-signal", "structured-result", "observed from structured result list");

        return items
            .GroupBy(item => item.FieldName, StringComparer.Ordinal)
            .Select(group => group
                .OrderBy(item => GetPriority(item.SourceType))
                .ThenBy(item => item.SourceType, StringComparer.Ordinal)
                .First())
            .OrderBy(item => item.FieldName, StringComparer.Ordinal)
            .ToArray();
    }

    private static void Add(
        ICollection<MotorYObservedFieldSourceSnapshot> items,
        IReadOnlyList<string>? fields,
        string sourceType,
        string sourceScope,
        string summaryPrefix)
    {
        if (fields is null)
        {
            return;
        }

        foreach (var field in fields
                     .Where(field => !string.IsNullOrWhiteSpace(field))
                     .Distinct(StringComparer.Ordinal))
        {
            items.Add(new MotorYObservedFieldSourceSnapshot
            {
                FieldName = field,
                SourceType = sourceType,
                SourceScope = sourceScope,
                SourceSummary = $"{field} {summaryPrefix}"
            });
        }
    }

    private static int GetPriority(string sourceType)
        => sourceType switch
        {
            "payload-field" => 0,
            "rated-param-field" => 1,
            "result-field" => 2,
            "intermediate-result-field" => 3,
            "raw-data-signal" => 4,
            "structured-payload-signal" => 5,
            "structured-result-signal" => 6,
            _ => int.MaxValue
        };
}
