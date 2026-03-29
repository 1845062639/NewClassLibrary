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
    public string SuggestedNextStepCategory { get; init; } = string.Empty;
    public string SuggestedNextStepFocus { get; init; } = string.Empty;
    public IReadOnlyList<string> SuggestedNextStepFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SuggestedNextSteps { get; init; } = Array.Empty<string>();
    public string SuggestedNextStepSummary { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

internal static class MotorYDecisionAnchorResolutionFactory
{
    private static (string Category, string Focus, IReadOnlyList<string> Fields) BuildResolutionSuggestionParts(string canonicalCode, string anchorKey, IReadOnlyList<string> missingPayloadFields)
    {
        var orderedFields = missingPayloadFields
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();

        if (orderedFields.Length == 0)
        {
            return (string.Empty, string.Empty, Array.Empty<string>());
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal))
        {
            return anchorKey switch
            {
                "rconverse-branch" => ("legacy-branch", "空载旧算法的 R0/θ0 换算分支标记", orderedFields),
                "pfw-fit-window" => ("fit-window", "空载低压段风摩损耗拟合结果", orderedFields),
                "rated-regression-ready" => ("regression-result", "空载 1.0pu 回归结果字段", orderedFields),
                _ => ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields)
            };
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal))
        {
            return anchorKey switch
            {
                "first-seconds-interval" => ("decision-interval", "热试验 firstSecondsInterval 判定依据", orderedFields),
                "hot-state-branch" => ("legacy-branch", "热试验 HotStateType 分支字段", orderedFields),
                "gb-temperature-branch" => ("legacy-branch", "热试验 GB 温升分支关键字段", orderedFields),
                _ => ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields)
            };
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
        {
            return anchorKey switch
            {
                "gb-ratios-branch" => ("legacy-branch", "B法 GB/ratios/θs 分支字段", orderedFields),
                "correlation-refit" => ("refit-evidence", "B法坏点剔除后二次拟合证据", orderedFields),
                "ps-iteration" => ("iteration-result", "B法 Ps 非负迭代收敛字段", orderedFields),
                "thermal-carryover" => ("upstream-carryover", "B法热态承接字段", orderedFields),
                _ => ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields)
            };
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.LockedRotor, StringComparison.Ordinal))
        {
            return anchorKey switch
            {
                "voltage-fit-branch" => ("fit-branch", "堵转电压拟合分支基准", orderedFields),
                "torquecal-branch" => ("legacy-branch", "堵转 TorqueCalType 分支字段", orderedFields),
                "rcal-branch" => ("legacy-branch", "堵转 RCalType/R1s 电阻分支字段", orderedFields),
                _ => ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields)
            };
        }

        if (string.Equals(canonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal))
        {
            return anchorKey switch
            {
                "upstream-ready" => ("upstream-carryover", "A法上游空载/热试验承接字段", orderedFields),
                "rated-load-fit-grid" => ("fit-grid", "A法额定负载点回归结果", orderedFields),
                "payload-rated-quantity-ready" => ("rated-quantity", "A法 payload 额定量结果字段", orderedFields),
                _ => ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields)
            };
        }

        return ("decision-anchor", $"决策锚点 {anchorKey}", orderedFields);
    }

    private static IReadOnlyList<string> BuildResolutionSuggestedNextSteps(string canonicalCode, string anchorKey, IReadOnlyList<string> missingPayloadFields, bool partial)
    {
        var suggestion = BuildResolutionSuggestionParts(canonicalCode, anchorKey, missingPayloadFields);
        if (suggestion.Fields.Count == 0)
        {
            return Array.Empty<string>();
        }

        var status = partial ? "继续补齐" : "先补";
        return new[]
        {
            $"{status}{suggestion.Focus}：{string.Join(", ", suggestion.Fields)}"
        };
    }

    public static IReadOnlyList<MotorYDecisionAnchorResolution> Build(
        string canonicalCode,
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
                    ? $"decision-anchor-resolution:{rule.AnchorKey} resolved {observed}/{total}"
                    : partial
                        ? $"decision-anchor-resolution:{rule.AnchorKey} partial {observed}/{total}; missing '{string.Join("', '", rule.MissingPayloadFields)}'"
                        : $"decision-anchor-resolution:{rule.AnchorKey} missing observed payload fields '{string.Join("', '", rule.MissingPayloadFields)}'";

                var suggestion = BuildResolutionSuggestionParts(canonicalCode, rule.AnchorKey, rule.MissingPayloadFields);
                var suggestedNextSteps = BuildResolutionSuggestedNextSteps(canonicalCode, rule.AnchorKey, rule.MissingPayloadFields, partial);
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
                    SuggestedNextStepCategory = suggestion.Category,
                    SuggestedNextStepFocus = suggestion.Focus,
                    SuggestedNextStepFields = suggestion.Fields,
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
            .Select(x => x.SuggestedNextSteps.FirstOrDefault())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
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

        return $"decision anchor next actions: {string.Join("; ", suggestedSteps.Select(ToEnglishActionClause))}";
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

    private static string ToEnglishActionClause(string step)
    {
        if (string.IsNullOrWhiteSpace(step))
        {
            return string.Empty;
        }

        return step
            .Replace("先补", "need ", StringComparison.Ordinal)
            .Replace("继续补齐", "continue filling ", StringComparison.Ordinal)
            .Replace("空载旧算法的 R0/θ0 换算分支标记：", "NoLoad R0/θ0 branch fields ", StringComparison.Ordinal)
            .Replace("空载低压段风摩损耗拟合结果：", "NoLoad Pfw fit fields ", StringComparison.Ordinal)
            .Replace("空载 1.0pu 回归结果字段：", "NoLoad 1.0pu regression fields ", StringComparison.Ordinal)
            .Replace("热试验 firstSecondsInterval 判定依据：", "HeatRun firstSecondsInterval fields ", StringComparison.Ordinal)
            .Replace("热试验 HotStateType 分支字段：", "HeatRun HotStateType fields ", StringComparison.Ordinal)
            .Replace("热试验 GB 温升分支关键字段：", "HeatRun GB temperature branch fields ", StringComparison.Ordinal)
            .Replace("B法 GB/ratios/θs 分支字段：", "LoadB GB/ratios/θs branch fields ", StringComparison.Ordinal)
            .Replace("B法坏点剔除后二次拟合证据：", "LoadB correlation refit fields ", StringComparison.Ordinal)
            .Replace("B法 Ps 非负迭代收敛字段：", "LoadB Ps iteration fields ", StringComparison.Ordinal)
            .Replace("B法热态承接字段：", "LoadB thermal carryover fields ", StringComparison.Ordinal)
            .Replace("堵转电压拟合分支基准：", "LockedRotor voltage-fit branch fields ", StringComparison.Ordinal)
            .Replace("堵转 TorqueCalType 分支字段：", "LockedRotor TorqueCalType fields ", StringComparison.Ordinal)
            .Replace("堵转 RCalType/R1s 电阻分支字段：", "LockedRotor RCalType/R1s fields ", StringComparison.Ordinal)
            .Replace("A法上游空载/热试验承接字段：", "LoadA upstream fields ", StringComparison.Ordinal)
            .Replace("A法额定负载点回归结果：", "LoadA rated-load fit fields ", StringComparison.Ordinal)
            .Replace("A法 payload 额定量结果字段：", "LoadA payload rated-result fields ", StringComparison.Ordinal);
    }
}
