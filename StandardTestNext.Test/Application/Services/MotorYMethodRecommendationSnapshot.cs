namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYMethodRecommendationSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public int BaselineMethod { get; init; }
    public int BaselineCount { get; init; }
    public string BaselineMethodKey { get; init; } = string.Empty;
    public string? BaselineProfileKey { get; init; }
    public int DominantMethod { get; init; }
    public int DominantCount { get; init; }
    public string DominantMethodKey { get; init; } = string.Empty;
    public string? DominantProfileKey { get; init; }
    public string? DominantVariantKind { get; init; }
    public string? DominantAlgorithmFamily { get; init; }
    public string? DominantLegacyEnumName { get; init; }
    public string? DominantLegacyFormName { get; init; }
    public string? DominantLegacyAlgorithmEntry { get; init; }
    public string? DominantLegacyMethodName { get; init; }
    public string? DominantLegacySettingsMethodName { get; init; }
    public bool DominantIsBaselineMethod { get; init; }
    public bool ShouldPrioritizeDominantOverBaseline { get; init; }
    public double DominantShare { get; init; }
    public double BaselineShare { get; init; }
    public int RecommendedMethod { get; init; }
    public string RecommendedMethodKey { get; init; } = string.Empty;
    public string? RecommendedProfileKey { get; init; }
    public string? RecommendedVariantKind { get; init; }
    public string? RecommendedAlgorithmFamily { get; init; }
    public string? RecommendedLegacyEnumName { get; init; }
    public string? RecommendedLegacyFormName { get; init; }
    public string? RecommendedLegacyAlgorithmEntry { get; init; }
    public string? RecommendedLegacyMethodName { get; init; }
    public string? RecommendedLegacySettingsMethodName { get; init; }
    public bool RecommendedIsBaselineMethod { get; init; }
    public bool RecommendedIsDominantMethod { get; init; }
    public string RecommendedStrategy { get; init; } = string.Empty;
    public string RecommendationReason { get; init; } = string.Empty;
    public string RecommendedMethodSummary { get; init; } = string.Empty;
    public string BaselineDominantComparisonSummary { get; init; } = string.Empty;
}
