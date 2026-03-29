namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYObservedAlgorithmEvidenceGap
{
    public string SignalOrRule { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public bool CoveredByObservedPayload { get; init; }
    public string Summary { get; init; } = string.Empty;
}

internal sealed class MotorYObservedAlgorithmEvidenceSnapshot
{
    public bool BackedByObservedPayload { get; init; }
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public IReadOnlyList<MotorYObservedAlgorithmEvidenceGap> SignalOrRuleGaps { get; init; } = Array.Empty<MotorYObservedAlgorithmEvidenceGap>();
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

    private static readonly IReadOnlyDictionary<string, string[]> LegacyDecisionAnchorObservedFieldsByCanonicalCode =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new[] { "R1", "θ1c" },
            [MotorYTestMethodCodes.NoLoad] = new[] { "RConverseType", "Pfw", "CoefficientOfPfe", "I0", "ΔI0", "P0", "Pcu", "Pfe" },
            [MotorYTestMethodCodes.HeatRun] = new[] { "Pn", "HotStateType", "GB", "Rn", "θw", "θs", "θb" },
            [MotorYTestMethodCodes.LoadA] = new[] { "CoefficientOfPfe", "Pfw", "θa", "Pcu1", "Pcu2", "η", "ResultDataList" },
            [MotorYTestMethodCodes.LoadB] = new[] { "GB", "θw", "θb", "θs", "A", "B", "R", "Ps", "ResultDataList" },
            [MotorYTestMethodCodes.LockedRotor] = new[] { "TorqueCalType", "RCalType", "R1s", "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn", "Un" }
        };

    public static MotorYObservedAlgorithmEvidenceSnapshot BuildFormulaSignalEvidence(
        string canonicalCode,
        IReadOnlyList<string>? observedPayloadFields,
        IReadOnlyList<string>? observedStructuredSignals = null)
        => Build(canonicalCode, observedPayloadFields, observedStructuredSignals, FormulaSignalObservedFieldsByCanonicalCode, "formula signal observed payload", "formula-signal");

    public static MotorYObservedAlgorithmEvidenceSnapshot BuildLegacyRuleEvidence(
        string canonicalCode,
        IReadOnlyList<string>? observedPayloadFields,
        IReadOnlyList<string>? observedStructuredSignals = null)
        => Build(canonicalCode, observedPayloadFields, observedStructuredSignals, LegacyRuleObservedFieldsByCanonicalCode, "legacy algorithm rule observed payload", "legacy-rule");

    public static MotorYObservedAlgorithmEvidenceSnapshot BuildLegacyDecisionAnchorEvidence(
        string canonicalCode,
        IReadOnlyList<string>? observedPayloadFields,
        IReadOnlyList<string>? observedStructuredSignals = null)
        => Build(canonicalCode, observedPayloadFields, observedStructuredSignals, LegacyDecisionAnchorObservedFieldsByCanonicalCode, "legacy decision anchor observed payload", "decision-anchor");

    private static MotorYObservedAlgorithmEvidenceSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string>? observedPayloadFields,
        IReadOnlyList<string>? observedStructuredSignals,
        IReadOnlyDictionary<string, string[]> catalog,
        string summaryLabel,
        string gapLabelPrefix)
    {
        var requiredFields = catalog.TryGetValue(canonicalCode, out var fields)
            ? fields.Where(field => !string.IsNullOrWhiteSpace(field)).Distinct(StringComparer.Ordinal).ToArray()
            : Array.Empty<string>();
        var observed = (observedPayloadFields ?? Array.Empty<string>())
            .Concat(observedStructuredSignals ?? Array.Empty<string>())
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var matched = requiredFields
            .Where(field => observed.Contains(field, StringComparer.Ordinal))
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var missing = requiredFields
            .Where(field => !matched.Contains(field, StringComparer.Ordinal))
            .ToArray();
        var ratio = requiredFields.Length == 0
            ? 1d
            : Math.Round((double)matched.Length / requiredFields.Length, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);
        var gaps = BuildSignalOrRuleGaps(requiredFields, observed, gapLabelPrefix);

        return new MotorYObservedAlgorithmEvidenceSnapshot
        {
            BackedByObservedPayload = requiredFields.Length == 0 || matched.Length > 0,
            ObservedPayloadFields = matched,
            MissingPayloadFields = missing,
            CoverageRatio = ratio,
            CoveragePercentagePoints = percentagePoints,
            SignalOrRuleGaps = gaps,
            Summary = $"{summaryLabel} fields observed {matched.Length}/{requiredFields.Length} ({percentagePoints}pp); missing: {(missing.Length == 0 ? "none" : string.Join(", ", missing))}; observed: {(matched.Length == 0 ? "none" : string.Join(", ", matched))}"
        };
    }

    private static IReadOnlyList<MotorYObservedAlgorithmEvidenceGap> BuildSignalOrRuleGaps(
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> observedFields,
        string gapLabelPrefix)
    {
        return requiredFields
            .OrderBy(field => field, StringComparer.Ordinal)
            .Select(field =>
            {
                var isCovered = observedFields.Contains(field, StringComparer.Ordinal);
                return new MotorYObservedAlgorithmEvidenceGap
                {
                    SignalOrRule = $"{gapLabelPrefix}:{field}",
                    RequiredPayloadFields = new[] { field },
                    ObservedPayloadFields = isCovered ? new[] { field } : Array.Empty<string>(),
                    MissingPayloadFields = isCovered ? Array.Empty<string>() : new[] { field },
                    CoveredByObservedPayload = isCovered,
                    Summary = isCovered
                        ? $"{gapLabelPrefix}:{field} covered by observed payload field '{field}'"
                        : $"{gapLabelPrefix}:{field} missing observed payload field '{field}'"
                };
            })
            .ToArray();
    }
}
