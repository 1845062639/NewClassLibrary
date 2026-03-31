namespace StandardTestNext.Test.Application.Services;

internal static class MotorYPrimaryFieldFocusFactory
{
    private static readonly string[] EmptyStrings = Array.Empty<string>();

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildCrossPlanPrimaryFieldFocuses(
            plans,
            plan => plan.DecisionAnchorPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                plan.SelectedRoute?.VariantKind ?? string.Empty,
                plan.SelectedRoute?.MethodValue,
                plan.SelectedRoute?.MethodKey ?? string.Empty,
                plan.SelectedRoute?.ProfileKey ?? string.Empty,
                plan.LegacyMethodName,
                plan.SettingsMethodName,
                GetLegacyBusinessCodes(plan),
                plan.AlgorithmEntry,
                GetLegacyEnumNames(plan),
                GetLegacyFormNames(plan),
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                plan.UpstreamCanonicalCodes,
                string.Join(" | ", plan.UpstreamLegacyCodeDistributions.Select(x => $"{x.CanonicalCode}:{x.LegacyCode}:{x.Count}:{x.Share:0.####}")),
                Array.Empty<string>(),
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities,
                plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
                plan.BaselineRoute?.MethodValue,
                plan.BaselineRoute?.MethodKey ?? string.Empty,
                plan.BaselineRoute?.ProfileKey ?? string.Empty,
                plan.DominantRoute?.MethodValue,
                plan.DominantRoute?.MethodKey ?? string.Empty,
                plan.DominantRoute?.ProfileKey ?? string.Empty,
                plan.SelectedRoute?.MethodValue,
                plan.SelectedRoute?.MethodKey ?? string.Empty,
                plan.SelectedRoute?.ProfileKey ?? string.Empty)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildCrossPlanRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildCrossPlanPrimaryFieldFocuses(
            plans,
            plan => plan.RequiredResultPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
                distribution.PrimaryField,
                plan.AlgorithmFamily,
                plan.SelectedRoute?.VariantKind ?? string.Empty,
                plan.SelectedRoute?.MethodValue,
                plan.SelectedRoute?.MethodKey ?? string.Empty,
                plan.SelectedRoute?.ProfileKey ?? string.Empty,
                plan.LegacyMethodName,
                plan.SettingsMethodName,
                GetLegacyBusinessCodes(plan),
                plan.AlgorithmEntry,
                GetLegacyEnumNames(plan),
                GetLegacyFormNames(plan),
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                Array.Empty<string>(),
                string.Join(" | ", distribution.DisplayNames),
                Array.Empty<string>(),
                distribution.BucketKeys,
                Array.Empty<string>(),
                Array.Empty<string>(),
                plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
                plan.BaselineRoute?.MethodValue,
                plan.BaselineRoute?.MethodKey ?? string.Empty,
                plan.BaselineRoute?.ProfileKey ?? string.Empty,
                plan.DominantRoute?.MethodValue,
                plan.DominantRoute?.MethodKey ?? string.Empty,
                plan.DominantRoute?.ProfileKey ?? string.Empty,
                plan.SelectedRoute?.MethodValue,
                plan.SelectedRoute?.MethodKey ?? string.Empty,
                plan.SelectedRoute?.ProfileKey ?? string.Empty)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(plans, plan => plan.DecisionAnchorPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
            distribution.PrimaryField,
            plan.AlgorithmFamily,
            plan.SelectedRoute?.VariantKind ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty,
            plan.LegacyMethodName,
            plan.SettingsMethodName,
            GetLegacyBusinessCodes(plan),
            plan.AlgorithmEntry,
            GetLegacyEnumNames(plan),
            GetLegacyFormNames(plan),
            plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
            plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
            plan.UpstreamCanonicalCodes,
            string.Join(" | ", plan.UpstreamLegacyCodeDistributions.Select(x => $"{x.CanonicalCode}:{x.LegacyCode}:{x.Count}:{x.Share:0.####}")),
            Array.Empty<string>(),
            distribution.AnchorKeys,
            distribution.SuggestedNextStepFocuses,
            distribution.SuggestedNextStepPriorities,
            plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
            plan.BaselineRoute?.MethodValue,
            plan.BaselineRoute?.MethodKey ?? string.Empty,
            plan.BaselineRoute?.ProfileKey ?? string.Empty,
            plan.DominantRoute?.MethodValue,
            plan.DominantRoute?.MethodKey ?? string.Empty,
            plan.DominantRoute?.ProfileKey ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(plans, plan => plan.RequiredResultPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
            distribution.PrimaryField,
            plan.AlgorithmFamily,
            plan.SelectedRoute?.VariantKind ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty,
            plan.LegacyMethodName,
            plan.SettingsMethodName,
            GetLegacyBusinessCodes(plan),
            plan.AlgorithmEntry,
            GetLegacyEnumNames(plan),
            GetLegacyFormNames(plan),
            plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
            plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
            Array.Empty<string>(),
            string.Join(" | ", distribution.DisplayNames),
            Array.Empty<string>(),
            distribution.BucketKeys,
            Array.Empty<string>(),
            Array.Empty<string>(),
            plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
            plan.BaselineRoute?.MethodValue,
            plan.BaselineRoute?.MethodKey ?? string.Empty,
            plan.BaselineRoute?.ProfileKey ?? string.Empty,
            plan.DominantRoute?.MethodValue,
            plan.DominantRoute?.MethodKey ?? string.Empty,
            plan.DominantRoute?.ProfileKey ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildVariantKindDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildVariantKindPrimaryFieldFocuses(plans, plan => plan.DecisionAnchorPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
            distribution.PrimaryField,
            plan.AlgorithmFamily,
            plan.SelectedRoute?.VariantKind ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty,
            plan.LegacyMethodName,
            plan.SettingsMethodName,
            GetLegacyBusinessCodes(plan),
            plan.AlgorithmEntry,
            GetLegacyEnumNames(plan),
            GetLegacyFormNames(plan),
            plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
            plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
            plan.UpstreamCanonicalCodes,
            string.Join(" | ", plan.UpstreamLegacyCodeDistributions.Select(x => $"{x.CanonicalCode}:{x.LegacyCode}:{x.Count}:{x.Share:0.####}")),
            Array.Empty<string>(),
            distribution.AnchorKeys,
            distribution.SuggestedNextStepFocuses,
            distribution.SuggestedNextStepPriorities,
            plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
            plan.BaselineRoute?.MethodValue,
            plan.BaselineRoute?.MethodKey ?? string.Empty,
            plan.BaselineRoute?.ProfileKey ?? string.Empty,
            plan.DominantRoute?.MethodValue,
            plan.DominantRoute?.MethodKey ?? string.Empty,
            plan.DominantRoute?.ProfileKey ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildVariantKindRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildVariantKindPrimaryFieldFocuses(plans, plan => plan.RequiredResultPrimaryFieldDistributions.Select(distribution => new CrossPlanPrimaryFieldCandidate(
            distribution.PrimaryField,
            plan.AlgorithmFamily,
            plan.SelectedRoute?.VariantKind ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty,
            plan.LegacyMethodName,
            plan.SettingsMethodName,
            GetLegacyBusinessCodes(plan),
            plan.AlgorithmEntry,
            GetLegacyEnumNames(plan),
            GetLegacyFormNames(plan),
            plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
            plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
            plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
            Array.Empty<string>(),
            string.Join(" | ", distribution.DisplayNames),
            Array.Empty<string>(),
            distribution.BucketKeys,
            Array.Empty<string>(),
            Array.Empty<string>(),
            plan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
            plan.BaselineRoute?.MethodValue,
            plan.BaselineRoute?.MethodKey ?? string.Empty,
            plan.BaselineRoute?.ProfileKey ?? string.Empty,
            plan.DominantRoute?.MethodValue,
            plan.DominantRoute?.MethodKey ?? string.Empty,
            plan.DominantRoute?.ProfileKey ?? string.Empty,
            plan.SelectedRoute?.MethodValue,
            plan.SelectedRoute?.MethodKey ?? string.Empty,
            plan.SelectedRoute?.ProfileKey ?? string.Empty)));

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
                    BaselineCount = focus.BaselineCount,
                    BaselineShare = focus.BaselineShare,
                    BaselineWeightedCount = focus.BaselineWeightedCount,
                    BaselineWeightedShare = focus.BaselineWeightedShare,
                    DominantCount = focus.DominantCount,
                    DominantShare = focus.DominantShare,
                    DominantWeightedCount = focus.DominantWeightedCount,
                    DominantWeightedShare = focus.DominantWeightedShare,
                    SelectedCount = focus.SelectedCount,
                    SelectedShare = focus.SelectedShare,
                    SelectedWeightedCount = focus.SelectedWeightedCount,
                    SelectedWeightedShare = focus.SelectedWeightedShare,
                    BaselineMethodValue = focus.BaselineMethodValue,
                    BaselineMethodKey = focus.BaselineMethodKey,
                    BaselineProfileKey = focus.BaselineProfileKey,
                    DominantMethodValue = focus.DominantMethodValue,
                    DominantMethodKey = focus.DominantMethodKey,
                    DominantProfileKey = focus.DominantProfileKey,
                    SelectedMethodValue = focus.SelectedMethodValue,
                    SelectedMethodKey = focus.SelectedMethodKey,
                    SelectedProfileKey = focus.SelectedProfileKey,
                    CanonicalCodes = focus.CanonicalCodes,
                    AlgorithmFamilies = new[] { group.Key },
                    VariantKinds = focus.VariantKinds,
                    MethodValues = focus.MethodValues,
                    MethodKeys = focus.MethodKeys,
                    ProfileKeys = focus.ProfileKeys,
                    LegacyMethodNames = focus.LegacyMethodNames,
                    SettingsMethodNames = focus.SettingsMethodNames,
                    LegacyAlgorithmEntries = focus.LegacyAlgorithmEntries,
                    LegacyEnumNames = focus.LegacyEnumNames,
                    LegacyFormNames = focus.LegacyFormNames,
                    SourceSections = focus.SourceSections,
                    SourceRanges = focus.SourceRanges,
                    FormNames = focus.FormNames,
                    FormSourceRanges = focus.FormSourceRanges,
                    UpstreamCanonicalCodes = focus.UpstreamCanonicalCodes,
                    UpstreamSummaryHints = focus.UpstreamSummaryHints,
                    AnchorKeys = focus.AnchorKeys,
                    SuggestedNextStepFocuses = focus.SuggestedNextStepFocuses,
                    SuggestedNextStepPriorities = focus.SuggestedNextStepPriorities,
                    UpstreamLegacyCodes = focus.UpstreamLegacyCodes,
                    Summary = $"family={group.Key}; {focus.Summary}"
                });
            })
            .ToArray();
    }

    private static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildVariantKindPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        Func<MotorYMethodAdaptationPlanSnapshot, IEnumerable<CrossPlanPrimaryFieldCandidate>> candidateSelector)
    {
        if (plans.Count == 0)
        {
            return Array.Empty<MotorYPrimaryFieldFocusSnapshot>();
        }

        return plans
            .Where(plan => !string.IsNullOrWhiteSpace(plan.SelectedRoute?.VariantKind))
            .GroupBy(plan => plan.SelectedRoute?.VariantKind ?? string.Empty, StringComparer.Ordinal)
            .OrderBy(group => GetVariantKindSortOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .SelectMany(group =>
            {
                var variantPlans = group.ToArray();
                var variantFocuses = BuildCrossPlanPrimaryFieldFocuses(variantPlans, candidateSelector);
                return variantFocuses.Select(focus => new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = focus.PrimaryField,
                    Count = focus.Count,
                    Share = focus.Share,
                    WeightedCount = focus.WeightedCount,
                    WeightedShare = focus.WeightedShare,
                    BaselineCount = focus.BaselineCount,
                    BaselineShare = focus.BaselineShare,
                    BaselineWeightedCount = focus.BaselineWeightedCount,
                    BaselineWeightedShare = focus.BaselineWeightedShare,
                    DominantCount = focus.DominantCount,
                    DominantShare = focus.DominantShare,
                    DominantWeightedCount = focus.DominantWeightedCount,
                    DominantWeightedShare = focus.DominantWeightedShare,
                    SelectedCount = focus.SelectedCount,
                    SelectedShare = focus.SelectedShare,
                    SelectedWeightedCount = focus.SelectedWeightedCount,
                    SelectedWeightedShare = focus.SelectedWeightedShare,
                    BaselineMethodValue = focus.BaselineMethodValue,
                    BaselineMethodKey = focus.BaselineMethodKey,
                    BaselineProfileKey = focus.BaselineProfileKey,
                    DominantMethodValue = focus.DominantMethodValue,
                    DominantMethodKey = focus.DominantMethodKey,
                    DominantProfileKey = focus.DominantProfileKey,
                    SelectedMethodValue = focus.SelectedMethodValue,
                    SelectedMethodKey = focus.SelectedMethodKey,
                    SelectedProfileKey = focus.SelectedProfileKey,
                    CanonicalCodes = focus.CanonicalCodes,
                    AlgorithmFamilies = focus.AlgorithmFamilies,
                    VariantKinds = new[] { group.Key },
                    MethodValues = focus.MethodValues,
                    MethodKeys = focus.MethodKeys,
                    ProfileKeys = focus.ProfileKeys,
                    LegacyMethodNames = focus.LegacyMethodNames,
                    SettingsMethodNames = focus.SettingsMethodNames,
                    LegacyAlgorithmEntries = focus.LegacyAlgorithmEntries,
                    LegacyEnumNames = focus.LegacyEnumNames,
                    LegacyFormNames = focus.LegacyFormNames,
                    SourceSections = focus.SourceSections,
                    SourceRanges = focus.SourceRanges,
                    FormNames = focus.FormNames,
                    FormSourceRanges = focus.FormSourceRanges,
                    UpstreamCanonicalCodes = focus.UpstreamCanonicalCodes,
                    UpstreamSummaryHints = focus.UpstreamSummaryHints,
                    AnchorKeys = focus.AnchorKeys,
                    SuggestedNextStepFocuses = focus.SuggestedNextStepFocuses,
                    SuggestedNextStepPriorities = focus.SuggestedNextStepPriorities,
                    UpstreamLegacyCodes = focus.UpstreamLegacyCodes,
                    Summary = $"variant={group.Key}; {focus.Summary}"
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
                var rows = group.OrderBy(x => x.CanonicalCode, StringComparer.Ordinal).ToArray();
                var share = Math.Round((double)rows.Length / total, 4, MidpointRounding.AwayFromZero);
                var weightedCount = rows.Sum(x => x.Weight);
                var weightedShare = totalWeighted <= 0 ? 0d : Math.Round((double)weightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
                var baselineCount = rows.Count(x => x.Candidate.VariantKind == MotorYLegacyVariantKinds.Baseline);
                var baselineShare = rows.Length == 0 ? 0d : Math.Round((double)baselineCount / rows.Length, 4, MidpointRounding.AwayFromZero);
                var baselineWeightedCount = rows.Where(x => x.Candidate.VariantKind == MotorYLegacyVariantKinds.Baseline).Sum(x => x.Weight);
                var baselineWeightedShare = totalWeighted <= 0 ? 0d : Math.Round((double)baselineWeightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
                var dominantCount = rows.Count(x => x.Candidate.MethodKey == x.Candidate.DominantMethodKey);
                var dominantShare = rows.Length == 0 ? 0d : Math.Round((double)dominantCount / rows.Length, 4, MidpointRounding.AwayFromZero);
                var dominantWeightedCount = rows.Where(x => x.Candidate.MethodKey == x.Candidate.DominantMethodKey).Sum(x => x.Weight);
                var dominantWeightedShare = totalWeighted <= 0 ? 0d : Math.Round((double)dominantWeightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
                var selectedCount = rows.Count(x => x.Candidate.MethodKey == x.Candidate.SelectedMethodKey);
                var selectedShare = rows.Length == 0 ? 0d : Math.Round((double)selectedCount / rows.Length, 4, MidpointRounding.AwayFromZero);
                var selectedWeightedCount = rows.Where(x => x.Candidate.MethodKey == x.Candidate.SelectedMethodKey).Sum(x => x.Weight);
                var selectedWeightedShare = totalWeighted <= 0 ? 0d : Math.Round((double)selectedWeightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
                var canonicalCodes = rows.Select(x => x.CanonicalCode).Distinct(StringComparer.Ordinal).ToArray();
                var legacyItemAliases = canonicalCodes
                    .SelectMany(MotorYLegacyItemCodeNormalizer.GetLegacyAliases)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToArray();
                var algorithmFamilies = rows.Select(x => x.Candidate.AlgorithmFamily).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var variantKinds = rows.Select(x => x.Candidate.VariantKind).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(GetVariantKindSortOrder).ThenBy(x => x, StringComparer.Ordinal).ToArray();
                var methodValues = rows.Where(x => x.Candidate.MethodValue.HasValue).Select(x => x.Candidate.MethodValue!.Value).Distinct().OrderBy(x => x).ToArray();
                var methodKeys = rows.Select(x => x.Candidate.MethodKey).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var profileKeys = rows.Select(x => x.Candidate.ProfileKey).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyMethodNames = rows.Select(x => x.Candidate.LegacyMethodName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var settingsMethodNames = rows.Select(x => x.Candidate.SettingsMethodName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyBusinessCodes = rows.SelectMany(x => x.Candidate.LegacyBusinessCodes).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyAlgorithmEntries = rows.Select(x => x.Candidate.AlgorithmEntry).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyEnumNames = rows.SelectMany(x => x.Candidate.LegacyEnumNames).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyFormNames = rows.SelectMany(x => x.Candidate.LegacyFormNames).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var dominantLegacyAlgorithmEntry = rows.Select(x => x.Candidate.AlgorithmEntry)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .FirstOrDefault()?.Key ?? string.Empty;
                var sourceSections = rows.SelectMany(x => x.Candidate.SourceSections).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var sourceRanges = rows.SelectMany(x => x.Candidate.SourceRanges).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var dominantSourceSection = rows.SelectMany(x => x.Candidate.SourceSections)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .FirstOrDefault()?.Key ?? string.Empty;
                var dominantSourceRange = rows.SelectMany(x => x.Candidate.SourceRanges)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .FirstOrDefault()?.Key ?? string.Empty;
                var formNames = rows.SelectMany(x => x.Candidate.FormNames).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var formSourceRanges = rows.SelectMany(x => x.Candidate.FormSourceRanges).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var dominantFormName = rows.SelectMany(x => x.Candidate.FormNames)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .FirstOrDefault()?.Key ?? string.Empty;
                var dominantFormSourceRange = rows.SelectMany(x => x.Candidate.FormSourceRanges)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.Ordinal)
                    .OrderByDescending(x => x.Count())
                    .ThenBy(x => x.Key, StringComparer.Ordinal)
                    .FirstOrDefault()?.Key ?? string.Empty;
                var formEvidenceSummary = formNames.Length == 0 ? "none" : string.Join(", ", formNames.Select((name, index) =>
                {
                    var range = index < formSourceRanges.Length ? formSourceRanges[index] : string.Empty;
                    return string.IsNullOrWhiteSpace(range) ? name : $"{name}({range})";
                }));
                var upstreamCanonicalCodes = rows.SelectMany(x => x.Candidate.UpstreamCanonicalCodes).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var upstreamSummaryHints = rows.Select(x => x.Candidate.UpstreamSummaryHint).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var upstreamLegacyCodes = rows.SelectMany(x => x.Candidate.UpstreamLegacyCodes).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var anchorKeys = rows.SelectMany(x => x.Candidate.AnchorKeys).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var focuses = rows.SelectMany(x => x.Candidate.Focuses).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var priorities = rows.SelectMany(x => x.Candidate.Priorities).Distinct(StringComparer.Ordinal).OrderBy(GetPrioritySortOrder).ThenBy(x => x, StringComparer.Ordinal).ToArray();
                var percentagePoints = (int)Math.Round(share * 100d, MidpointRounding.AwayFromZero);
                var weightedPercentagePoints = (int)Math.Round(weightedShare * 100d, MidpointRounding.AwayFromZero);
                var baselineMethodValue = rows.Select(x => x.Candidate.BaselineMethodValue).Where(x => x.HasValue).Select(x => x!.Value).GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).FirstOrDefault()?.Key;
                var baselineMethodKey = rows.Select(x => x.Candidate.BaselineMethodKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var baselineProfileKey = rows.Select(x => x.Candidate.BaselineProfileKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var dominantMethodValue = rows.Select(x => x.Candidate.DominantMethodValue).Where(x => x.HasValue).Select(x => x!.Value).GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).FirstOrDefault()?.Key;
                var dominantMethodKey = rows.Select(x => x.Candidate.DominantMethodKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var dominantProfileKey = rows.Select(x => x.Candidate.DominantProfileKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var selectedMethodValue = rows.Select(x => x.Candidate.SelectedMethodValue).Where(x => x.HasValue).Select(x => x!.Value).GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).FirstOrDefault()?.Key;
                var selectedMethodKey = rows.Select(x => x.Candidate.SelectedMethodKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var selectedProfileKey = rows.Select(x => x.Candidate.SelectedProfileKey).Where(x => !string.IsNullOrWhiteSpace(x)).GroupBy(x => x, StringComparer.Ordinal).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? string.Empty;
                var methodValueSummary = methodValues.Length == 0 ? "none" : string.Join(", ", methodValues);
                var methodKeySummary = methodKeys.Length == 0 ? "none" : string.Join(", ", methodKeys);
                var profileKeySummary = profileKeys.Length == 0 ? "none" : string.Join(", ", profileKeys);
                var legacyMethodNameSummary = legacyMethodNames.Length == 0 ? "none" : string.Join(", ", legacyMethodNames);
                var settingsMethodNameSummary = settingsMethodNames.Length == 0 ? "none" : string.Join(", ", settingsMethodNames);
                var legacyAlgorithmEntrySummary = legacyAlgorithmEntries.Length == 0 ? "none" : string.Join(", ", legacyAlgorithmEntries);
                var legacyBusinessCodeSummary = legacyBusinessCodes.Length == 0 ? "none" : string.Join(", ", legacyBusinessCodes);
                var legacyItemAliasSummary = legacyItemAliases.Length == 0 ? "none" : string.Join(", ", legacyItemAliases);
                var legacyEnumNameSummary = legacyEnumNames.Length == 0 ? "none" : string.Join(", ", legacyEnumNames);
                var legacyFormNameSummary = legacyFormNames.Length == 0 ? "none" : string.Join(", ", legacyFormNames);
                var sourceSectionSummary = sourceSections.Length == 0 ? "none" : string.Join(", ", sourceSections);
                var sourceRangeSummary = sourceRanges.Length == 0 ? "none" : string.Join(", ", sourceRanges);
                var formNameSummary = formNames.Length == 0 ? "none" : string.Join(", ", formNames);
                var formSourceRangeSummary = formSourceRanges.Length == 0 ? "none" : string.Join(", ", formSourceRanges);
                var upstreamCanonicalCodeSummary = upstreamCanonicalCodes.Length == 0 ? "none" : string.Join(", ", upstreamCanonicalCodes);
                var upstreamHintSummary = upstreamSummaryHints.Length == 0 ? "none" : string.Join(" | ", upstreamSummaryHints);
                var upstreamLegacyCodeSummary = upstreamLegacyCodes.Length == 0 ? "none" : string.Join(", ", upstreamLegacyCodes);
                var summary = $"cross-plan primary field {group.Key} appears in {rows.Length}/{total} plans ({percentagePoints}pp), weighted {weightedCount}/{totalWeighted} selected samples ({weightedPercentagePoints}pp); baseline={baselineCount}/{rows.Length} ({(int)Math.Round(baselineShare * 100d, MidpointRounding.AwayFromZero)}pp, weighted {baselineWeightedCount}/{totalWeighted} => {(int)Math.Round(baselineWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp) => {FormatRoutePreview(baselineMethodValue, baselineMethodKey, baselineProfileKey)}; dominant={dominantCount}/{rows.Length} ({(int)Math.Round(dominantShare * 100d, MidpointRounding.AwayFromZero)}pp, weighted {dominantWeightedCount}/{totalWeighted} => {(int)Math.Round(dominantWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp) => {FormatRoutePreview(dominantMethodValue, dominantMethodKey, dominantProfileKey)}; selected={selectedCount}/{rows.Length} ({(int)Math.Round(selectedShare * 100d, MidpointRounding.AwayFromZero)}pp, weighted {selectedWeightedCount}/{totalWeighted} => {(int)Math.Round(selectedWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp) => {FormatRoutePreview(selectedMethodValue, selectedMethodKey, selectedProfileKey)}; codes={string.Join(", ", canonicalCodes)}; legacy-item-aliases={legacyItemAliasSummary}; methods={methodValueSummary}; method-keys={methodKeySummary}; profiles={profileKeySummary}; legacy-methods={legacyMethodNameSummary}; settings-methods={settingsMethodNameSummary}; legacy-business-codes={legacyBusinessCodeSummary}; legacy-enums={legacyEnumNameSummary}; legacy-forms={legacyFormNameSummary}; algo-entries={legacyAlgorithmEntrySummary}; dominant-algo-entry={(string.IsNullOrWhiteSpace(dominantLegacyAlgorithmEntry) ? "none" : dominantLegacyAlgorithmEntry)}; source-sections={sourceSectionSummary}; source-ranges={sourceRangeSummary}; dominant-source={(string.IsNullOrWhiteSpace(dominantSourceSection) ? "none" : dominantSourceSection)}@{(string.IsNullOrWhiteSpace(dominantSourceRange) ? "none" : dominantSourceRange)}; forms={formNameSummary}; form-ranges={formSourceRangeSummary}; dominant-form={(string.IsNullOrWhiteSpace(dominantFormName) ? "none" : dominantFormName)}@{(string.IsNullOrWhiteSpace(dominantFormSourceRange) ? "none" : dominantFormSourceRange)}; form-evidence={formEvidenceSummary}; upstream-codes={upstreamCanonicalCodeSummary}; upstream-hints={upstreamHintSummary}; upstream-legacy={upstreamLegacyCodeSummary}; families={(algorithmFamilies.Length == 0 ? "none" : string.Join(", ", algorithmFamilies))}; variants={(variantKinds.Length == 0 ? "none" : string.Join(", ", variantKinds))}; focuses={(focuses.Length == 0 ? "none" : string.Join(", ", focuses))}; priorities={(priorities.Length == 0 ? "none" : string.Join(", ", priorities))}";


                return new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = group.Key,
                    Count = rows.Length,
                    Share = share,
                    WeightedCount = weightedCount,
                    WeightedShare = weightedShare,
                    BaselineCount = baselineCount,
                    BaselineShare = baselineShare,
                    BaselineWeightedCount = baselineWeightedCount,
                    BaselineWeightedShare = baselineWeightedShare,
                    DominantCount = dominantCount,
                    DominantShare = dominantShare,
                    DominantWeightedCount = dominantWeightedCount,
                    DominantWeightedShare = dominantWeightedShare,
                    SelectedCount = selectedCount,
                    SelectedShare = selectedShare,
                    SelectedWeightedCount = selectedWeightedCount,
                    SelectedWeightedShare = selectedWeightedShare,
                    BaselineMethodValue = baselineMethodValue,
                    BaselineMethodKey = baselineMethodKey,
                    BaselineProfileKey = baselineProfileKey,
                    DominantMethodValue = dominantMethodValue,
                    DominantMethodKey = dominantMethodKey,
                    DominantProfileKey = dominantProfileKey,
                    SelectedMethodValue = selectedMethodValue,
                    SelectedMethodKey = selectedMethodKey,
                    SelectedProfileKey = selectedProfileKey,
                    CanonicalCodes = canonicalCodes,
                    LegacyItemAliases = legacyItemAliases,
                    AlgorithmFamilies = algorithmFamilies,
                    VariantKinds = variantKinds,
                    MethodValues = methodValues,
                    MethodKeys = methodKeys,
                    ProfileKeys = profileKeys,
                    LegacyMethodNames = legacyMethodNames,
                    SettingsMethodNames = settingsMethodNames,
                    LegacyBusinessCodes = legacyBusinessCodes,
                    LegacyAlgorithmEntries = legacyAlgorithmEntries,
                    LegacyEnumNames = legacyEnumNames,
                    LegacyFormNames = legacyFormNames,
                    DominantLegacyAlgorithmEntry = dominantLegacyAlgorithmEntry,
                    SourceSections = sourceSections,
                    SourceRanges = sourceRanges,
                    DominantSourceSection = dominantSourceSection,
                    DominantSourceRange = dominantSourceRange,
                    FormNames = formNames,
                    FormSourceRanges = formSourceRanges,
                    DominantFormName = dominantFormName,
                    DominantFormSourceRange = dominantFormSourceRange,
                    UpstreamCanonicalCodes = upstreamCanonicalCodes,
                    UpstreamSummaryHints = upstreamSummaryHints,
                    UpstreamLegacyCodes = upstreamLegacyCodes,
                    AnchorKeys = anchorKeys,
                    SuggestedNextStepFocuses = focuses,
                    SuggestedNextStepPriorities = priorities,
                    Summary = summary
                };
            })
            .ToArray();
    }

    public static string BuildCrossPlanFocusSummary(string scope, IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        if (focuses.Count == 0)
        {
            return $"cross-plan {scope} primary fields: none";
        }

        var preview = focuses.Take(3).Select(x => $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp)").ToArray();
        var top = focuses[0];
        var topFamilies = top.AlgorithmFamilies.Count == 0 ? "none" : string.Join("/", top.AlgorithmFamilies);
        var topCodes = top.CanonicalCodes.Count == 0 ? "none" : string.Join("/", top.CanonicalCodes.Take(3));
        var topLegacyItemAliases = top.LegacyItemAliases.Count == 0 ? "none" : string.Join("/", top.LegacyItemAliases.Take(6));
        var topMethodValues = top.MethodValues.Count == 0 ? "none" : string.Join("/", top.MethodValues);
        var topMethodKeys = top.MethodKeys.Count == 0 ? "none" : string.Join("/", top.MethodKeys);
        var topLegacyMethods = top.LegacyMethodNames.Count == 0 ? "none" : string.Join("/", top.LegacyMethodNames);
        var topSettingsMethods = top.SettingsMethodNames.Count == 0 ? "none" : string.Join("/", top.SettingsMethodNames);
        var topLegacyBusinessCodes = top.LegacyBusinessCodes.Count == 0 ? "none" : string.Join("/", top.LegacyBusinessCodes);
        var topLegacyEnums = top.LegacyEnumNames.Count == 0 ? "none" : string.Join("/", top.LegacyEnumNames);
        var topLegacyForms = top.LegacyFormNames.Count == 0 ? "none" : string.Join("/", top.LegacyFormNames);
        var topLegacyAlgorithmEntries = top.LegacyAlgorithmEntries.Count == 0 ? "none" : string.Join("/", top.LegacyAlgorithmEntries);
        var topSourceSections = top.SourceSections.Count == 0 ? "none" : string.Join("/", top.SourceSections);
        var topSourceRanges = top.SourceRanges.Count == 0 ? "none" : string.Join("/", top.SourceRanges);
        var topFormNames = top.FormNames.Count == 0 ? "none" : string.Join("/", top.FormNames);
        var topFormRanges = top.FormSourceRanges.Count == 0 ? "none" : string.Join("/", top.FormSourceRanges);
        var topDominantSource = (string.IsNullOrWhiteSpace(top.DominantSourceSection) ? "none" : top.DominantSourceSection)
            + "@"
            + (string.IsNullOrWhiteSpace(top.DominantSourceRange) ? "none" : top.DominantSourceRange);
        var topDominantForm = (string.IsNullOrWhiteSpace(top.DominantFormName) ? "none" : top.DominantFormName)
            + "@"
            + (string.IsNullOrWhiteSpace(top.DominantFormSourceRange) ? "none" : top.DominantFormSourceRange);
        var topUpstreamCodes = top.UpstreamCanonicalCodes.Count == 0 ? "none" : string.Join("/", top.UpstreamCanonicalCodes);
        var topUpstreamLegacyCodes = top.UpstreamLegacyCodes.Count == 0 ? "none" : string.Join("/", top.UpstreamLegacyCodes);
        var topUpstreamHints = top.UpstreamSummaryHints.Count == 0 ? "none" : string.Join("/", top.UpstreamSummaryHints);
        var topVariants = top.VariantKinds.Count == 0 ? "none" : string.Join("/", top.VariantKinds);
        var topProfiles = top.ProfileKeys.Count == 0 ? "none" : string.Join("/", top.ProfileKeys);
        var baselineShare = $"{top.BaselineCount}/{top.Count} ({(int)Math.Round(top.BaselineShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        var baselineWeightedShare = top.WeightedCount <= 0
            ? "0/0 (0pp)"
            : $"{top.BaselineWeightedCount}/{top.WeightedCount} ({(int)Math.Round(top.BaselineWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        var dominantShare = $"{top.DominantCount}/{top.Count} ({(int)Math.Round(top.DominantShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        var dominantWeightedShare = top.WeightedCount <= 0
            ? "0/0 (0pp)"
            : $"{top.DominantWeightedCount}/{top.WeightedCount} ({(int)Math.Round(top.DominantWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        var selectedShare = $"{top.SelectedCount}/{top.Count} ({(int)Math.Round(top.SelectedShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        var selectedWeightedShare = top.WeightedCount <= 0
            ? "0/0 (0pp)"
            : $"{top.SelectedWeightedCount}/{top.WeightedCount} ({(int)Math.Round(top.SelectedWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp)";
        return $"cross-plan {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant={top.PrimaryField}@families={topFamilies}@variants={topVariants}@codes={topCodes}@legacy-item-aliases={topLegacyItemAliases}@methods={topMethodValues}@method-keys={topMethodKeys}@profiles={topProfiles}@legacy-methods={topLegacyMethods}@settings-methods={topSettingsMethods}@legacy-business-codes={topLegacyBusinessCodes}@legacy-enums={topLegacyEnums}@legacy-forms={topLegacyForms}@algo-entries={topLegacyAlgorithmEntries}@source-sections={topSourceSections}@source-ranges={topSourceRanges}@forms={topFormNames}@form-ranges={topFormRanges}@dominant-source={topDominantSource}@dominant-form={topDominantForm}@baseline={baselineShare}@baseline-weighted={baselineWeightedShare}@dominant-share={dominantShare}@dominant-weighted={dominantWeightedShare}@selected-share={selectedShare}@selected-weighted={selectedWeightedShare}@upstream={topUpstreamCodes}@upstream-legacy={topUpstreamLegacyCodes}@upstream-hints={topUpstreamHints}";
    }

    public static string BuildAlgorithmFamilyFocusSummary(string scope, IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        if (focuses.Count == 0)
        {
            return $"algorithm-family {scope} primary fields: none";
        }

        var preview = focuses.Take(3).Select(x =>
        {
            var familyLabel = x.AlgorithmFamilies.Count == 0 ? "no-family" : string.Join("/", x.AlgorithmFamilies);
            return $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp, families {familyLabel})";
        }).ToArray();

        var dominantFamily = focuses.SelectMany(focus => focus.AlgorithmFamilies).Where(family => !string.IsNullOrWhiteSpace(family)).GroupBy(family => family, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantMethodValues = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.MethodValues).Distinct().OrderBy(value => value).ToArray();
        var dominantMethodKeys = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.MethodKeys).Where(methodKey => !string.IsNullOrWhiteSpace(methodKey)).Distinct(StringComparer.Ordinal).OrderBy(methodKey => methodKey, StringComparer.Ordinal).ToArray();
        var dominantLegacyMethodNames = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyMethodNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantSettingsMethodNames = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.SettingsMethodNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyBusinessCodes = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyBusinessCodes).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantCanonicalCodes = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.CanonicalCodes).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        var dominantLegacyEnumNames = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyEnumNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyFormNames = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyFormNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyAlgorithmEntries = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyAlgorithmEntries).Where(entry => !string.IsNullOrWhiteSpace(entry)).Distinct(StringComparer.Ordinal).OrderBy(entry => entry, StringComparer.Ordinal).ToArray();
        var dominantSourceSections = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.SourceSections).Where(section => !string.IsNullOrWhiteSpace(section)).Distinct(StringComparer.Ordinal).OrderBy(section => section, StringComparer.Ordinal).ToArray();
        var dominantSourceRanges = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.SourceRanges).Where(range => !string.IsNullOrWhiteSpace(range)).Distinct(StringComparer.Ordinal).OrderBy(range => range, StringComparer.Ordinal).ToArray();
        var dominantDominantAlgorithmEntry = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).Select(focus => focus.DominantLegacyAlgorithmEntry).Where(entry => !string.IsNullOrWhiteSpace(entry)).GroupBy(entry => entry, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantSourceSection = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).Select(focus => focus.DominantSourceSection).Where(section => !string.IsNullOrWhiteSpace(section)).GroupBy(section => section, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantSourceRange = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).Select(focus => focus.DominantSourceRange).Where(range => !string.IsNullOrWhiteSpace(range)).GroupBy(range => range, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantFormNames = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.FormNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantFormRanges = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.FormSourceRanges).Where(range => !string.IsNullOrWhiteSpace(range)).Distinct(StringComparer.Ordinal).OrderBy(range => range, StringComparer.Ordinal).ToArray();
        var dominantDominantFormName = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).Select(focus => focus.DominantFormName).Where(name => !string.IsNullOrWhiteSpace(name)).GroupBy(name => name, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantFormRange = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).Select(focus => focus.DominantFormSourceRange).Where(range => !string.IsNullOrWhiteSpace(range)).GroupBy(range => range, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantUpstreamLegacyCodes = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.UpstreamLegacyCodes).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        var dominantUpstreamHints = focuses.Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal)).SelectMany(focus => focus.UpstreamSummaryHints).Where(hint => !string.IsNullOrWhiteSpace(hint)).Distinct(StringComparer.Ordinal).OrderBy(hint => hint, StringComparer.Ordinal).ToArray();
        return $"algorithm-family {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant-family={dominantFamily}@codes={(dominantCanonicalCodes.Length == 0 ? "none" : string.Join("/", dominantCanonicalCodes))}@methods={(dominantMethodValues.Length == 0 ? "none" : string.Join("/", dominantMethodValues))}@method-keys={(dominantMethodKeys.Length == 0 ? "none" : string.Join("/", dominantMethodKeys))}@legacy-methods={(dominantLegacyMethodNames.Length == 0 ? "none" : string.Join("/", dominantLegacyMethodNames))}@settings-methods={(dominantSettingsMethodNames.Length == 0 ? "none" : string.Join("/", dominantSettingsMethodNames))}@legacy-business-codes={(dominantLegacyBusinessCodes.Length == 0 ? "none" : string.Join("/", dominantLegacyBusinessCodes))}@legacy-enums={(dominantLegacyEnumNames.Length == 0 ? "none" : string.Join("/", dominantLegacyEnumNames))}@legacy-forms={(dominantLegacyFormNames.Length == 0 ? "none" : string.Join("/", dominantLegacyFormNames))}@algo-entries={(dominantLegacyAlgorithmEntries.Length == 0 ? "none" : string.Join("/", dominantLegacyAlgorithmEntries))}@dominant-algo={dominantDominantAlgorithmEntry}@source-sections={(dominantSourceSections.Length == 0 ? "none" : string.Join("/", dominantSourceSections))}@source-ranges={(dominantSourceRanges.Length == 0 ? "none" : string.Join("/", dominantSourceRanges))}@dominant-source={dominantDominantSourceSection}@{dominantDominantSourceRange}@forms={(dominantFormNames.Length == 0 ? "none" : string.Join("/", dominantFormNames))}@form-ranges={(dominantFormRanges.Length == 0 ? "none" : string.Join("/", dominantFormRanges))}@dominant-form={dominantDominantFormName}@{dominantDominantFormRange}@upstream-legacy={(dominantUpstreamLegacyCodes.Length == 0 ? "none" : string.Join("/", dominantUpstreamLegacyCodes))}@upstream-hints={(dominantUpstreamHints.Length == 0 ? "none" : string.Join("/", dominantUpstreamHints))}";
    }

    public static string BuildVariantKindFocusSummary(string scope, IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        if (focuses.Count == 0)
        {
            return $"variant-kind {scope} primary fields: none";
        }

        var preview = focuses.Take(3).Select(x =>
        {
            var variantLabel = x.VariantKinds.Count == 0 ? "no-variant" : string.Join("/", x.VariantKinds);
            return $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp, variants {variantLabel})";
        }).ToArray();

        var dominantVariant = focuses.SelectMany(focus => focus.VariantKinds).Where(variant => !string.IsNullOrWhiteSpace(variant)).GroupBy(variant => variant, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => GetVariantKindSortOrder(group.Key)).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantMethodValues = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.MethodValues).Distinct().OrderBy(value => value).ToArray();
        var dominantMethodKeys = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.MethodKeys).Where(methodKey => !string.IsNullOrWhiteSpace(methodKey)).Distinct(StringComparer.Ordinal).OrderBy(methodKey => methodKey, StringComparer.Ordinal).ToArray();
        var dominantLegacyMethodNames = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyMethodNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantSettingsMethodNames = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.SettingsMethodNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyBusinessCodes = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyBusinessCodes).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantCanonicalCodes = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.CanonicalCodes).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        var dominantLegacyEnumNames = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyEnumNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyFormNames = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyFormNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantLegacyAlgorithmEntries = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.LegacyAlgorithmEntries).Where(entry => !string.IsNullOrWhiteSpace(entry)).Distinct(StringComparer.Ordinal).OrderBy(entry => entry, StringComparer.Ordinal).ToArray();
        var dominantSourceSections = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.SourceSections).Where(section => !string.IsNullOrWhiteSpace(section)).Distinct(StringComparer.Ordinal).OrderBy(section => section, StringComparer.Ordinal).ToArray();
        var dominantSourceRanges = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.SourceRanges).Where(range => !string.IsNullOrWhiteSpace(range)).Distinct(StringComparer.Ordinal).OrderBy(range => range, StringComparer.Ordinal).ToArray();
        var dominantDominantAlgorithmEntry = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).Select(focus => focus.DominantLegacyAlgorithmEntry).Where(entry => !string.IsNullOrWhiteSpace(entry)).GroupBy(entry => entry, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantSourceSection = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).Select(focus => focus.DominantSourceSection).Where(section => !string.IsNullOrWhiteSpace(section)).GroupBy(section => section, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantSourceRange = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).Select(focus => focus.DominantSourceRange).Where(range => !string.IsNullOrWhiteSpace(range)).GroupBy(range => range, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantFormNames = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.FormNames).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var dominantFormRanges = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.FormSourceRanges).Where(range => !string.IsNullOrWhiteSpace(range)).Distinct(StringComparer.Ordinal).OrderBy(range => range, StringComparer.Ordinal).ToArray();
        var dominantDominantFormName = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).Select(focus => focus.DominantFormName).Where(name => !string.IsNullOrWhiteSpace(name)).GroupBy(name => name, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantDominantFormRange = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).Select(focus => focus.DominantFormSourceRange).Where(range => !string.IsNullOrWhiteSpace(range)).GroupBy(range => range, StringComparer.Ordinal).OrderByDescending(group => group.Count()).ThenBy(group => group.Key, StringComparer.Ordinal).FirstOrDefault()?.Key ?? "none";
        var dominantUpstreamLegacyCodes = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.UpstreamLegacyCodes).Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        var dominantUpstreamHints = focuses.Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal)).SelectMany(focus => focus.UpstreamSummaryHints).Where(hint => !string.IsNullOrWhiteSpace(hint)).Distinct(StringComparer.Ordinal).OrderBy(hint => hint, StringComparer.Ordinal).ToArray();
        return $"variant-kind {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant-variant={dominantVariant}@codes={(dominantCanonicalCodes.Length == 0 ? "none" : string.Join("/", dominantCanonicalCodes))}@methods={(dominantMethodValues.Length == 0 ? "none" : string.Join("/", dominantMethodValues))}@method-keys={(dominantMethodKeys.Length == 0 ? "none" : string.Join("/", dominantMethodKeys))}@legacy-methods={(dominantLegacyMethodNames.Length == 0 ? "none" : string.Join("/", dominantLegacyMethodNames))}@settings-methods={(dominantSettingsMethodNames.Length == 0 ? "none" : string.Join("/", dominantSettingsMethodNames))}@legacy-business-codes={(dominantLegacyBusinessCodes.Length == 0 ? "none" : string.Join("/", dominantLegacyBusinessCodes))}@legacy-enums={(dominantLegacyEnumNames.Length == 0 ? "none" : string.Join("/", dominantLegacyEnumNames))}@legacy-forms={(dominantLegacyFormNames.Length == 0 ? "none" : string.Join("/", dominantLegacyFormNames))}@algo-entries={(dominantLegacyAlgorithmEntries.Length == 0 ? "none" : string.Join("/", dominantLegacyAlgorithmEntries))}@dominant-algo={dominantDominantAlgorithmEntry}@source-sections={(dominantSourceSections.Length == 0 ? "none" : string.Join("/", dominantSourceSections))}@source-ranges={(dominantSourceRanges.Length == 0 ? "none" : string.Join("/", dominantSourceRanges))}@dominant-source={dominantDominantSourceSection}@{dominantDominantSourceRange}@forms={(dominantFormNames.Length == 0 ? "none" : string.Join("/", dominantFormNames))}@form-ranges={(dominantFormRanges.Length == 0 ? "none" : string.Join("/", dominantFormRanges))}@dominant-form={dominantDominantFormName}@{dominantDominantFormRange}@upstream-legacy={(dominantUpstreamLegacyCodes.Length == 0 ? "none" : string.Join("/", dominantUpstreamLegacyCodes))}@upstream-hints={(dominantUpstreamHints.Length == 0 ? "none" : string.Join("/", dominantUpstreamHints))}";
    }

    private static string FormatRoutePreview(int? methodValue, string methodKey, string profileKey)
    {
        var parts = new List<string>();
        if (methodValue.HasValue) parts.Add($"method={methodValue.Value}");
        if (!string.IsNullOrWhiteSpace(methodKey)) parts.Add($"method-key={methodKey}");
        if (!string.IsNullOrWhiteSpace(profileKey)) parts.Add($"profile={profileKey}");
        return parts.Count == 0 ? "none" : string.Join(", ", parts);
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

    private static int GetVariantKindSortOrder(string variantKind)
        => variantKind switch
        {
            MotorYLegacyVariantKinds.Baseline => 0,
            MotorYLegacyVariantKinds.Delivery => 1,
            MotorYLegacyVariantKinds.Companion => 2,
            MotorYLegacyVariantKinds.LegacyAlias => 3,
            _ => 99
        };

    private sealed record CrossPlanPrimaryFieldCandidate(
        string PrimaryField,
        string AlgorithmFamily,
        string VariantKind,
        int? MethodValue,
        string MethodKey,
        string ProfileKey,
        string LegacyMethodName,
        string SettingsMethodName,
        IReadOnlyList<string> LegacyBusinessCodes,
        string AlgorithmEntry,
        IReadOnlyList<string> LegacyEnumNames,
        IReadOnlyList<string> LegacyFormNames,
        IReadOnlyList<string> SourceSections,
        IReadOnlyList<string> SourceRanges,
        IReadOnlyList<string> FormNames,
        IReadOnlyList<string> FormSourceRanges,
        IReadOnlyList<string> UpstreamCanonicalCodes,
        string UpstreamSummaryHint,
        IReadOnlyList<string> DisplayNames,
        IReadOnlyList<string> UpstreamLegacyCodes,
        IReadOnlyList<string> AnchorKeys,
        IReadOnlyList<string> Focuses,
        IReadOnlyList<string> Priorities,
        int? BaselineMethodValue,
        string BaselineMethodKey,
        string BaselineProfileKey,
        int? DominantMethodValue,
        string DominantMethodKey,
        string DominantProfileKey,
        int? SelectedMethodValue,
        string SelectedMethodKey,
        string SelectedProfileKey);

    private static IReadOnlyList<string> GetLegacyBusinessCodes(MotorYMethodAdaptationPlanSnapshot plan)
    {
        return DistinctNonEmpty(
            plan.LegacyMethodName,
            plan.DominantRoute?.LegacyMethodName,
            plan.BaselineRoute?.LegacyMethodName,
            plan.SelectedRoute?.LegacyMethodName);
    }

    private static IReadOnlyList<string> GetLegacyEnumNames(MotorYMethodAdaptationPlanSnapshot plan)
    {
        return DistinctNonEmpty(
            plan.SelectedRoute?.LegacyEnumName,
            plan.DominantRoute?.LegacyEnumName,
            plan.BaselineRoute?.LegacyEnumName);
    }

    private static IReadOnlyList<string> GetLegacyFormNames(MotorYMethodAdaptationPlanSnapshot plan)
    {
        return DistinctNonEmpty(
            plan.SelectedRoute?.LegacyFormName,
            plan.DominantRoute?.LegacyFormName,
            plan.BaselineRoute?.LegacyFormName);
    }

    private static IReadOnlyList<string> DistinctNonEmpty(params string?[] values)
    {
        var items = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return items.Length == 0 ? EmptyStrings : items;
    }

}
