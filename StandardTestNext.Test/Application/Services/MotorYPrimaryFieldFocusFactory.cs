namespace StandardTestNext.Test.Application.Services;

internal static class MotorYPrimaryFieldFocusFactory
{
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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities)));

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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
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
                    VariantKinds = focus.VariantKinds,
                    MethodValues = focus.MethodValues,
                    MethodKeys = focus.MethodKeys,
                    ProfileKeys = focus.ProfileKeys,
                    LegacyMethodNames = focus.LegacyMethodNames,
                    SettingsMethodNames = focus.SettingsMethodNames,
                    LegacyAlgorithmEntries = focus.LegacyAlgorithmEntries,
                    SourceSections = focus.SourceSections,
                    SourceRanges = focus.SourceRanges,
                    FormNames = focus.FormNames,
                    FormSourceRanges = focus.FormSourceRanges,
                    UpstreamCanonicalCodes = focus.UpstreamCanonicalCodes,
                    UpstreamSummaryHints = focus.UpstreamSummaryHints,
                    AnchorKeys = focus.AnchorKeys,
                    SuggestedNextStepFocuses = focus.SuggestedNextStepFocuses,
                    SuggestedNextStepPriorities = focus.SuggestedNextStepPriorities,
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
                    CanonicalCodes = focus.CanonicalCodes,
                    AlgorithmFamilies = focus.AlgorithmFamilies,
                    VariantKinds = new[] { group.Key },
                    MethodValues = focus.MethodValues,
                    MethodKeys = focus.MethodKeys,
                    ProfileKeys = focus.ProfileKeys,
                    LegacyMethodNames = focus.LegacyMethodNames,
                    SettingsMethodNames = focus.SettingsMethodNames,
                    LegacyAlgorithmEntries = focus.LegacyAlgorithmEntries,
                    SourceSections = focus.SourceSections,
                    SourceRanges = focus.SourceRanges,
                    FormNames = focus.FormNames,
                    FormSourceRanges = focus.FormSourceRanges,
                    UpstreamCanonicalCodes = focus.UpstreamCanonicalCodes,
                    UpstreamSummaryHints = focus.UpstreamSummaryHints,
                    AnchorKeys = focus.AnchorKeys,
                    SuggestedNextStepFocuses = focus.SuggestedNextStepFocuses,
                    SuggestedNextStepPriorities = focus.SuggestedNextStepPriorities,
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
                var variantKinds = rows.Select(x => x.Candidate.VariantKind).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(GetVariantKindSortOrder).ThenBy(x => x, StringComparer.Ordinal).ToArray();
                var methodValues = rows.Where(x => x.Candidate.MethodValue.HasValue).Select(x => x.Candidate.MethodValue!.Value).Distinct().OrderBy(x => x).ToArray();
                var methodKeys = rows.Select(x => x.Candidate.MethodKey).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var profileKeys = rows.Select(x => x.Candidate.ProfileKey).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyMethodNames = rows.Select(x => x.Candidate.LegacyMethodName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var settingsMethodNames = rows.Select(x => x.Candidate.SettingsMethodName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var legacyAlgorithmEntries = rows.Select(x => x.Candidate.AlgorithmEntry).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var sourceSections = rows.SelectMany(x => x.Candidate.SourceSections).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var sourceRanges = rows.SelectMany(x => x.Candidate.SourceRanges).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var formNames = rows.SelectMany(x => x.Candidate.FormNames).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var formSourceRanges = rows.SelectMany(x => x.Candidate.FormSourceRanges).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var formEvidenceSummary = formNames.Length == 0
                    ? "none"
                    : string.Join(", ", formNames.Select((name, index) =>
                    {
                        var range = index < formSourceRanges.Length ? formSourceRanges[index] : string.Empty;
                        return string.IsNullOrWhiteSpace(range)
                            ? name
                            : $"{name}({range})";
                    }));
                var upstreamCanonicalCodes = rows.SelectMany(x => x.Candidate.UpstreamCanonicalCodes).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var upstreamSummaryHints = rows.Select(x => x.Candidate.UpstreamSummaryHint).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var anchorKeys = rows.SelectMany(x => x.Candidate.AnchorKeys).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var focuses = rows.SelectMany(x => x.Candidate.Focuses).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var priorities = rows.SelectMany(x => x.Candidate.Priorities).Distinct(StringComparer.Ordinal).OrderBy(GetPrioritySortOrder).ThenBy(x => x, StringComparer.Ordinal).ToArray();
                var percentagePoints = (int)Math.Round(share * 100d, MidpointRounding.AwayFromZero);
                var weightedPercentagePoints = (int)Math.Round(weightedShare * 100d, MidpointRounding.AwayFromZero);
                var methodValueSummary = methodValues.Length == 0 ? "none" : string.Join(", ", methodValues);
                var methodKeySummary = methodKeys.Length == 0 ? "none" : string.Join(", ", methodKeys);
                var profileKeySummary = profileKeys.Length == 0 ? "none" : string.Join(", ", profileKeys);
                var legacyMethodNameSummary = legacyMethodNames.Length == 0 ? "none" : string.Join(", ", legacyMethodNames);
                var settingsMethodNameSummary = settingsMethodNames.Length == 0 ? "none" : string.Join(", ", settingsMethodNames);
                var legacyAlgorithmEntrySummary = legacyAlgorithmEntries.Length == 0 ? "none" : string.Join(", ", legacyAlgorithmEntries);
                var sourceSectionSummary = sourceSections.Length == 0 ? "none" : string.Join(", ", sourceSections);
                var sourceRangeSummary = sourceRanges.Length == 0 ? "none" : string.Join(", ", sourceRanges);
                var formNameSummary = formNames.Length == 0 ? "none" : string.Join(", ", formNames);
                var formSourceRangeSummary = formSourceRanges.Length == 0 ? "none" : string.Join(", ", formSourceRanges);
                var upstreamCanonicalCodeSummary = upstreamCanonicalCodes.Length == 0 ? "none" : string.Join(", ", upstreamCanonicalCodes);
                var upstreamHintSummary = upstreamSummaryHints.Length == 0 ? "none" : string.Join(" | ", upstreamSummaryHints);
                var summary = $"cross-plan primary field {group.Key} appears in {rows.Length}/{total} plans ({percentagePoints}pp), weighted {weightedCount}/{totalWeighted} selected samples ({weightedPercentagePoints}pp); codes={string.Join(", ", canonicalCodes)}; methods={methodValueSummary}; method-keys={methodKeySummary}; profiles={profileKeySummary}; legacy-methods={legacyMethodNameSummary}; settings-methods={settingsMethodNameSummary}; algo-entries={legacyAlgorithmEntrySummary}; source-sections={sourceSectionSummary}; source-ranges={sourceRangeSummary}; forms={formNameSummary}; form-ranges={formSourceRangeSummary}; form-evidence={formEvidenceSummary}; upstream-codes={upstreamCanonicalCodeSummary}; upstream-hints={upstreamHintSummary}; families={(algorithmFamilies.Length == 0 ? "none" : string.Join(", ", algorithmFamilies))}; variants={(variantKinds.Length == 0 ? "none" : string.Join(", ", variantKinds))}; focuses={(focuses.Length == 0 ? "none" : string.Join(", ", focuses))}; priorities={(priorities.Length == 0 ? "none" : string.Join(", ", priorities))}";

                return new MotorYPrimaryFieldFocusSnapshot
                {
                    PrimaryField = group.Key,
                    Count = rows.Length,
                    Share = share,
                    WeightedCount = weightedCount,
                    WeightedShare = weightedShare,
                    CanonicalCodes = canonicalCodes,
                    AlgorithmFamilies = algorithmFamilies,
                    VariantKinds = variantKinds,
                    MethodValues = methodValues,
                    MethodKeys = methodKeys,
                    ProfileKeys = profileKeys,
                    LegacyMethodNames = legacyMethodNames,
                    SettingsMethodNames = settingsMethodNames,
                    LegacyAlgorithmEntries = legacyAlgorithmEntries,
                    SourceSections = sourceSections,
                    SourceRanges = sourceRanges,
                    FormNames = formNames,
                    FormSourceRanges = formSourceRanges,
                    UpstreamCanonicalCodes = upstreamCanonicalCodes,
                    UpstreamSummaryHints = upstreamSummaryHints,
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

        var preview = focuses
            .Take(3)
            .Select(x => $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp)")
            .ToArray();

        var top = focuses[0];
        var topFamilies = top.AlgorithmFamilies.Count == 0
            ? "none"
            : string.Join("/", top.AlgorithmFamilies);
        var topCodes = top.CanonicalCodes.Count == 0
            ? "none"
            : string.Join("/", top.CanonicalCodes.Take(3));

        var topMethodKeys = top.MethodKeys.Count == 0
            ? "none"
            : string.Join("/", top.MethodKeys);
        var topLegacyMethods = top.LegacyMethodNames.Count == 0
            ? "none"
            : string.Join("/", top.LegacyMethodNames);
        var topSettingsMethods = top.SettingsMethodNames.Count == 0
            ? "none"
            : string.Join("/", top.SettingsMethodNames);
        var topLegacyAlgorithmEntries = top.LegacyAlgorithmEntries.Count == 0
            ? "none"
            : string.Join("/", top.LegacyAlgorithmEntries);
        var topUpstreamCodes = top.UpstreamCanonicalCodes.Count == 0
            ? "none"
            : string.Join("/", top.UpstreamCanonicalCodes);
        var topUpstreamHints = top.UpstreamSummaryHints.Count == 0
            ? "none"
            : string.Join("/", top.UpstreamSummaryHints);

        return $"cross-plan {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant={top.PrimaryField}@families={topFamilies}@codes={topCodes}@method-keys={topMethodKeys}@legacy-methods={topLegacyMethods}@settings-methods={topSettingsMethods}@algo-entries={topLegacyAlgorithmEntries}@upstream={topUpstreamCodes}@upstream-hints={topUpstreamHints}";
    }

    public static string BuildAlgorithmFamilyFocusSummary(string scope, IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        if (focuses.Count == 0)
        {
            return $"algorithm-family {scope} primary fields: none";
        }

        var preview = focuses
            .Take(3)
            .Select(x =>
            {
                var familyLabel = x.AlgorithmFamilies.Count == 0
                    ? "no-family"
                    : string.Join("/", x.AlgorithmFamilies);
                return $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp, families {familyLabel})";
            })
            .ToArray();

        var dominantFamily = focuses
            .SelectMany(focus => focus.AlgorithmFamilies)
            .Where(family => !string.IsNullOrWhiteSpace(family))
            .GroupBy(family => family, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? "none";

        var dominantMethodKeys = focuses
            .Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal))
            .SelectMany(focus => focus.MethodKeys)
            .Where(methodKey => !string.IsNullOrWhiteSpace(methodKey))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(methodKey => methodKey, StringComparer.Ordinal)
            .ToArray();
        var dominantMethodKeySummary = dominantMethodKeys.Length == 0
            ? "none"
            : string.Join("/", dominantMethodKeys);
        var dominantLegacyMethodNames = focuses
            .Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal))
            .SelectMany(focus => focus.LegacyMethodNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var dominantLegacyMethodSummary = dominantLegacyMethodNames.Length == 0
            ? "none"
            : string.Join("/", dominantLegacyMethodNames);
        var dominantSettingsMethodNames = focuses
            .Where(focus => focus.AlgorithmFamilies.Contains(dominantFamily, StringComparer.Ordinal))
            .SelectMany(focus => focus.SettingsMethodNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var dominantSettingsMethodSummary = dominantSettingsMethodNames.Length == 0
            ? "none"
            : string.Join("/", dominantSettingsMethodNames);

        return $"algorithm-family {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant-family={dominantFamily}@method-keys={dominantMethodKeySummary}@legacy-methods={dominantLegacyMethodSummary}@settings-methods={dominantSettingsMethodSummary}";
    }

    public static string BuildVariantKindFocusSummary(string scope, IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        if (focuses.Count == 0)
        {
            return $"variant-kind {scope} primary fields: none";
        }

        var preview = focuses
            .Take(3)
            .Select(x =>
            {
                var variantLabel = x.VariantKinds.Count == 0
                    ? "no-variant"
                    : string.Join("/", x.VariantKinds);
                return $"{x.PrimaryField}={x.Count} ({(int)Math.Round(x.Share * 100d, MidpointRounding.AwayFromZero)}pp, weighted {(int)Math.Round(x.WeightedShare * 100d, MidpointRounding.AwayFromZero)}pp, variants {variantLabel})";
            })
            .ToArray();

        var dominantVariant = focuses
            .SelectMany(focus => focus.VariantKinds)
            .Where(variant => !string.IsNullOrWhiteSpace(variant))
            .GroupBy(variant => variant, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => GetVariantKindSortOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .FirstOrDefault()?.Key ?? "none";

        var dominantMethodKeys = focuses
            .Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal))
            .SelectMany(focus => focus.MethodKeys)
            .Where(methodKey => !string.IsNullOrWhiteSpace(methodKey))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(methodKey => methodKey, StringComparer.Ordinal)
            .ToArray();
        var dominantMethodKeySummary = dominantMethodKeys.Length == 0
            ? "none"
            : string.Join("/", dominantMethodKeys);
        var dominantLegacyMethodNames = focuses
            .Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal))
            .SelectMany(focus => focus.LegacyMethodNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var dominantLegacyMethodSummary = dominantLegacyMethodNames.Length == 0
            ? "none"
            : string.Join("/", dominantLegacyMethodNames);
        var dominantSettingsMethodNames = focuses
            .Where(focus => focus.VariantKinds.Contains(dominantVariant, StringComparer.Ordinal))
            .SelectMany(focus => focus.SettingsMethodNames)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var dominantSettingsMethodSummary = dominantSettingsMethodNames.Length == 0
            ? "none"
            : string.Join("/", dominantSettingsMethodNames);

        return $"variant-kind {scope} primary fields top {Math.Min(3, focuses.Count)}/{focuses.Count}: {string.Join("; ", preview)}; dominant-variant={dominantVariant}@method-keys={dominantMethodKeySummary}@legacy-methods={dominantLegacyMethodSummary}@settings-methods={dominantSettingsMethodSummary}";
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
            MotorYLegacyVariantKinds.DeliveryCompanion => 3,
            MotorYLegacyVariantKinds.LegacyAlias => 4,
            MotorYLegacyVariantKinds.OtherVariant => 5,
            _ => 99
        };

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(
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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                plan.UpstreamCanonicalCodes,
                plan.UpstreamDependencySummary,
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildAlgorithmFamilyPrimaryFieldFocuses(
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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                plan.UpstreamCanonicalCodes,
                plan.UpstreamDependencySummary,
                Array.Empty<string>(),
                distribution.DisplayNames,
                distribution.BucketKeys)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildVariantKindDecisionAnchorPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildVariantKindPrimaryFieldFocuses(
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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                plan.UpstreamCanonicalCodes,
                plan.UpstreamDependencySummary,
                distribution.AnchorKeys,
                distribution.SuggestedNextStepFocuses,
                distribution.SuggestedNextStepPriorities)));

    public static IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> BuildVariantKindRequiredResultPrimaryFieldFocuses(IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans)
        => BuildVariantKindPrimaryFieldFocuses(
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
                plan.AlgorithmEntry,
                plan.SourceEvidences.Select(x => x.SectionKey).ToArray(),
                plan.SourceEvidences.Select(x => x.SourceRange).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.FormName).ToArray(),
                plan.FormDependencyEvidences.Select(x => x.SourceRange).ToArray(),
                plan.UpstreamCanonicalCodes,
                plan.UpstreamDependencySummary,
                Array.Empty<string>(),
                distribution.DisplayNames,
                distribution.BucketKeys)));

    private sealed record CrossPlanPrimaryFieldCandidate(
        string PrimaryField,
        string AlgorithmFamily,
        string VariantKind,
        int? MethodValue,
        string MethodKey,
        string ProfileKey,
        string LegacyMethodName,
        string SettingsMethodName,
        string AlgorithmEntry,
        IReadOnlyList<string> SourceSections,
        IReadOnlyList<string> SourceRanges,
        IReadOnlyList<string> FormNames,
        IReadOnlyList<string> FormSourceRanges,
        IReadOnlyList<string> UpstreamCanonicalCodes,
        string UpstreamSummaryHint,
        IReadOnlyList<string> AnchorKeys,
        IReadOnlyList<string> Focuses,
        IReadOnlyList<string> Priorities);
}
