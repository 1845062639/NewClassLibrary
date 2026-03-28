namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYObservedAlgorithmEvidenceSnapshot
{
    public bool BackedByObservedPayload { get; init; }
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYObservedAlgorithmEvidenceCatalog
{
    private static readonly IReadOnlyDictionary<string, string[]> FormulaSignalObservedFieldsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new[] { "Ruv", "Rvw", "Rwu", "R1", "θ1c" },
            [MotorYTestMethodCodes.NoLoad] = new[] { "R0", "θ0", "Pfw", "Pfe", "CoefficientOfPfe" },
            [MotorYTestMethodCodes.HeatRun] = new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" },
            [MotorYTestMethodCodes.LoadA] = new[] { "Pcu1", "Pcu2", "η", "ResultDataList" },
            [MotorYTestMethodCodes.LoadB] = new[] { "A", "B", "R", "θs", "ResultDataList" },
            [MotorYTestMethodCodes.LockedRotor] = new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" }
        };

    private static readonly IReadOnlyDictionary<string, string[]> LegacyRuleObservedFieldsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new[] { "R1", "θ1c" },
            [MotorYTestMethodCodes.NoLoad] = new[] { "RConverseType", "Pfw", "CoefficientOfPfe", "I0", "ΔI0", "P0", "Pcu", "Pfe" },
            [MotorYTestMethodCodes.HeatRun] = new[] { "HotStateType", "GB", "Rn", "θw", "θs", "θb" },
            [MotorYTestMethodCodes.LoadA] = new[] { "CoefficientOfPfe", "θa", "Pcu1", "Pcu2", "η", "ResultDataList" },
            [MotorYTestMethodCodes.LoadB] = new[] { "GB", "θw", "θb", "θs", "A", "B", "R", "Ps", "ResultDataList" },
            [MotorYTestMethodCodes.LockedRotor] = new[] { "TorqueCalType", "RCalType", "R1s", "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" }
        };

    public static MotorYObservedAlgorithmEvidenceSnapshot BuildFormulaSignalEvidence(string canonicalCode, IReadOnlyList<string>? observedPayloadFields)
        => Build(canonicalCode, observedPayloadFields, FormulaSignalObservedFieldsByCanonicalCode, "formula signal observed payload");

    public static MotorYObservedAlgorithmEvidenceSnapshot BuildLegacyRuleEvidence(string canonicalCode, IReadOnlyList<string>? observedPayloadFields)
        => Build(canonicalCode, observedPayloadFields, LegacyRuleObservedFieldsByCanonicalCode, "legacy algorithm rule observed payload");

    private static MotorYObservedAlgorithmEvidenceSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string>? observedPayloadFields,
        IReadOnlyDictionary<string, string[]> catalog,
        string summaryLabel)
    {
        var requiredFields = catalog.TryGetValue(canonicalCode, out var fields)
            ? fields.Where(field => !string.IsNullOrWhiteSpace(field)).Distinct(StringComparer.Ordinal).ToArray()
            : Array.Empty<string>();
        var observed = (observedPayloadFields ?? Array.Empty<string>())
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var matched = requiredFields
            .Where(field => observed.Contains(field, StringComparer.Ordinal))
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();

        return new MotorYObservedAlgorithmEvidenceSnapshot
        {
            BackedByObservedPayload = requiredFields.Length == 0 || matched.Length > 0,
            ObservedPayloadFields = matched,
            Summary = $"{summaryLabel} fields observed {matched.Length}/{requiredFields.Length}; observed: {(matched.Length == 0 ? "none" : string.Join(", ", matched))}"
        };
    }
}
