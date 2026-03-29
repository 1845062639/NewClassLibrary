namespace StandardTestNext.Test.Application.Services;

internal sealed class MotorYDecisionAnchorResolution
{
    public string AnchorKey { get; init; } = string.Empty;
    public bool ResolvedByObservedPayload { get; init; }
    public bool PartiallyResolvedByObservedPayload { get; init; }
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ObservedPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingPayloadFields { get; init; } = Array.Empty<string>();
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public string ResolutionStage { get; init; } = string.Empty;
    public IReadOnlyList<string> SuggestedNextSteps { get; init; } = Array.Empty<string>();
    public string SuggestedNextStepSummary { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYDecisionAnchorResolutionFactory
{
    private static IReadOnlyList<string> BuildResolutionSuggestedNextSteps(string anchorKey, IReadOnlyList<string> missingPayloadFields, bool partial)
    {
        if (missingPayloadFields.Count == 0)
        {
            return Array.Empty<string>();
        }

        var orderedFields = missingPayloadFields
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();

        if (orderedFields.Length == 0)
        {
            return Array.Empty<string>();
        }

        var status = partial ? "继续补齐" : "先补";
        return new[]
        {
            $"{status}决策锚点 {anchorKey}: {string.Join(", ", orderedFields)}"
        };
    }

    public static IReadOnlyList<MotorYDecisionAnchorResolution> Build(
        IReadOnlyList<MotorYDecisionAnchorObservationRule> rules)
    {
        if (rules.Count == 0)
        {
            return Array.Empty<MotorYDecisionAnchorResolution>();
        }

        return rules
            .Select(rule =>
            {
                var total = rule.RequiredPayloadFields.Count;
                var observed = rule.ObservedPayloadFields.Count;
                var resolved = total == 0 || rule.CoveredByObservedPayload;
                var partial = !resolved && observed > 0;
                var ratio = total == 0
                    ? 1d
                    : Math.Round((double)observed / total, 4, MidpointRounding.AwayFromZero);
                var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);
                var stage = resolved
                    ? "resolved"
                    : partial
                        ? "partial"
                        : "missing";
                var summary = resolved
                    ? $"decision anchor '{rule.AnchorKey}' resolved by observed payload ({observed}/{total}, {percentagePoints}pp)"
                    : partial
                        ? $"decision anchor '{rule.AnchorKey}' partially resolved by observed payload ({observed}/{total}, {percentagePoints}pp); missing: {string.Join(", ", rule.MissingPayloadFields)}"
                        : $"decision anchor '{rule.AnchorKey}' unresolved by observed payload (0/{total}, 0pp); missing: {string.Join(", ", rule.MissingPayloadFields)}";

                var suggestedNextSteps = BuildResolutionSuggestedNextSteps(rule.AnchorKey, rule.MissingPayloadFields, partial);
                var suggestedNextStepSummary = suggestedNextSteps.Count == 0
                    ? "decision anchor already resolved"
                    : string.Join("; ", suggestedNextSteps);

                return new MotorYDecisionAnchorResolution
                {
                    AnchorKey = rule.AnchorKey,
                    ResolvedByObservedPayload = resolved,
                    PartiallyResolvedByObservedPayload = partial,
                    RequiredPayloadFields = rule.RequiredPayloadFields,
                    ObservedPayloadFields = rule.ObservedPayloadFields,
                    MissingPayloadFields = rule.MissingPayloadFields,
                    CoverageRatio = ratio,
                    CoveragePercentagePoints = percentagePoints,
                    ResolutionStage = stage,
                    SuggestedNextSteps = suggestedNextSteps,
                    SuggestedNextStepSummary = suggestedNextStepSummary,
                    Summary = summary
                };
            })
            .ToArray();
    }

    public static IReadOnlyList<string> BuildSuggestedNextSteps(IReadOnlyList<MotorYDecisionAnchorResolution> resolutions)
    {
        if (resolutions.Count == 0)
        {
            return Array.Empty<string>();
        }

        return resolutions
            .Where(x => !x.ResolvedByObservedPayload)
            .Select(x => new
            {
                x.AnchorKey,
                MissingFields = x.MissingPayloadFields
                    .Where(field => !string.IsNullOrWhiteSpace(field))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(field => field, StringComparer.Ordinal)
                    .ToArray()
            })
            .Where(x => x.MissingFields.Length > 0)
            .Select(x => $"补齐决策锚点 {x.AnchorKey} 所需字段：{string.Join(" / ", x.MissingFields)}")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public static string BuildNextActionSummary(IReadOnlyList<MotorYDecisionAnchorResolution> resolutions)
    {
        if (resolutions.Count == 0)
        {
            return "decision anchor next actions unavailable";
        }

        var suggestedSteps = BuildSuggestedNextSteps(resolutions);
        if (suggestedSteps.Count == 0)
        {
            return "decision anchors ready; no additional branch evidence required";
        }

        return $"decision anchor next actions: {string.Join("; ", suggestedSteps)}";
    }

    public static string BuildGapPreviewSummary(IReadOnlyList<MotorYDecisionAnchorResolution> resolutions)
    {
        if (resolutions.Count == 0)
        {
            return "decision anchor gap preview unavailable";
        }

        var preview = resolutions
            .Where(x => !x.ResolvedByObservedPayload)
            .OrderByDescending(x => x.MissingPayloadFields.Count)
            .ThenBy(x => x.AnchorKey, StringComparer.Ordinal)
            .Take(3)
            .Select(x => $"{x.AnchorKey}[{(x.PartiallyResolvedByObservedPayload ? "partial" : "missing")}]:{(x.MissingPayloadFields.Count == 0 ? "none" : string.Join(", ", x.MissingPayloadFields.Take(3)) + (x.MissingPayloadFields.Count > 3 ? ", ..." : string.Empty))}")
            .ToArray();

        return preview.Length == 0
            ? "decision anchor gaps: none"
            : $"decision anchor gaps: {string.Join("; ", preview)}";
    }

    public static string BuildSummary(IReadOnlyList<MotorYDecisionAnchorResolution> resolutions)
    {
        if (resolutions.Count == 0)
        {
            return "decision anchor resolutions unavailable";
        }

        var resolved = resolutions.Count(x => x.ResolvedByObservedPayload);
        var partial = resolutions.Count(x => x.PartiallyResolvedByObservedPayload);
        var missing = resolutions.Count - resolved - partial;
        var ratio = Math.Round((double)resolved / resolutions.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);
        var unresolvedKeys = resolutions
            .Where(x => !x.ResolvedByObservedPayload)
            .Select(x => $"{x.AnchorKey}:{x.ResolutionStage}")
            .ToArray();

        return $"decision anchor resolutions resolved {resolved}/{resolutions.Count} ({percentagePoints}pp); partial={partial}; missing={missing}; unresolved: {(unresolvedKeys.Length == 0 ? "none" : string.Join(", ", unresolvedKeys))}";
    }
}
