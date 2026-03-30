namespace StandardTestNext.Test.Application.Services;

internal static class MotorYPrimaryFieldFocusFactory
{
    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildCrossPlanPrimaryFieldFocuses(
            plans,
            plan => plan.DecisionAnchorPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildCrossPlanPrimaryFieldFocuses(
            plans,
            plan => plan.RequiredResultPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                Array.Empty<string>(),
                distribution.DisplayNames,
                distribution.BucketKeys)));

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        Func<MotorYMethodAdaptationPlanSnapshot, IEnumerable<CrossPlanPrimaryFieldCandidate>> candidateSelector)
    {
        if (plans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        return plans
            .Where(plan => !string.IsNullOrWhiteSpace(plan.AlgorithmFamily))
            .GroupBy(plan => plan.AlgorithmFamily, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .SelectMany(group =>
            {
                var familyPlans = group.ToArray();
                var familyFocuses = BuildCrossPlanPrimaryFieldFocuses(familyPlans, candidateSelector);
                return familyFocuses.Select(focus => new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = focus.PrimaryField,
                    Count = focus.Count,
                    Share = focus.Share,
                    WeightedCount = focus.WeightedCount,
                    WeightedShare = focus.WeightedShare,
                    CanonicalCodes = focus.CanonicalCodes,
                    AlgorithmFamilies = new[] { group.Key },
                    AnchorKeys = focus.AnchorKeys,
                    SuggestedNextStepFocuses = focus.SuggestedNextStepFocuses,
                    SuggestedNextStepPriorities = focus.SuggestedNextStepPriorities,
                    Summary = $"family={group.Key}; {focus.Summary}"
                });
            })
            .ToArray();
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        Func<MotorYMethodAdaptationPlanSnapshot, IEnumerable<CrossPlanPrimaryFieldCandidate>> candidateSelector)
    {
        if (plans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        var total = plans.Count;
        var totalWeighted = plans.Sum(plan => Math.Max(1, plan.SelectedCount));

        return plans
            .SelectMany(plan => candidateSelector(plan).Select(candidate => new
            {
                plan.CanonicalCode,
                Weight = Math.Max(1, plan.SelectedCount),
                Candidate = candidate
            }))
            .Where(x => !string.IsNullOrWhiteSpace(x.Candidate.PrimaryField))
            .GroupBy(x => x.Candidate.PrimaryField, StringComparer.Ordinal)
            .OrderByDescending(group => group.Sum(x => x.Weight))
            .ThenByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var rows = group
                    .OrderBy(x => x.CanonicalCode, StringComparer.Ordinal)
                    .ToArray();
                var share = Math.Round((double)rows.Length / total, 4, MidpointRounding.AwayFromZero);
                var weightedCount = rows.Sum(x => x.Weight);
                var weightedShare = totalWeighted <= 0
                    ? 0d
                    : Math.Round((double)weightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
                var canonicalCodes = rows.Select(x => x.CanonicalCode).Distinct(StringComparer.Ordinal).ToArray();
                var algorithmFamilies = rows.Select(x => x.Candidate.AlgorithmFamily).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var anchorKeys = rows.SelectMany(x => x.Candidate.AnchorKeys).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var focuses = rows.SelectMany(x => x.Candidate.Focuses).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var priorities = rows.SelectMany(x => x.Candidate.Priorities).Distinct(StringComparer.Ordinal).OrderBy(GetPrioritySortOrder).ThenBy(x => x, StringComparer.Ordinal).ToArray();
                var percentagePoints = (int)Math.Round(share * 100d, MidpointRounding.AwayFromZero);
                var weightedPercentagePoints = (int)Math.Round(weightedShare * 100d, MidpointRounding.AwayFromZero);
                var summary = $"cross-plan primary field {group.Key} appears in {rows.Length}/{total} plans ({percentagePoints}pp), weighted {weightedCount}/{totalWeighted} selected samples ({weightedPercentagePoints}pp); codes={string.Join(", ", canonicalCodes)}; families={(algorithmFamilies.Length == 0 ? "none" : string.Join(", ", algorithmFamilies))}; focuses={(focuses.Length == 0 ? "none" : string.Join(", ", focuses))}; priorities={(priorities.Length == 0 ? "none" : string.Join(", ", priorities))}";

                return new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = group.Key,
                    Count = rows.Length,
                    Share = share,
                    WeightedCount = weightedCount,
                    WeightedShare = weightedShare,
                    CanonicalCodes = canonicalCodes,
                    AlgorithmFamilies = algorithmFamilies,
                    AnchorKeys = anchorKeys,
                    SuggestedNextStepFocuses = focuses,
                    SuggestedNextStepPriorities = priorities,
                    Summary = summary
                };
            })
            .ToArray();
    }

    private static int GetPrioritySortOrder(string priority)
        => priority switch
        {
            "blocking" => 0,
            "follow-up" => 1,
            "resolved" => 2,
            "intermediate-result-fields" => 10,
            "result-fields" => 11,
            _ => 99
        };

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(
            plans,
            plan => plan.DecisionAnchorPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(
            plans,
            plan => plan.RequiredResultPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                Array.Empty<string>(),
                distribution.DisplayNames,
                distribution.BucketKeys)));

    private sealed record CrossPlanPrimaryFieldCandidate(
        string PrimaryField,
        string AlgorithmFamily,
        IReadOnlyList<string> AnchorKeys,
        IReadOnlyList<string> Focuses,
        IReadOnlyList<string> Priorities);
}
