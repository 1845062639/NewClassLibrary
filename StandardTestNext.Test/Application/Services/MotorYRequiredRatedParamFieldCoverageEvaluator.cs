using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

internal static class MotorYRequiredRatedParamFieldCoverageEvaluator
{
    public static MotorYRequiredRatedParamFieldCoverageSnapshot Evaluate(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        MotorRatedParamsContract? ratedParams)
    {
        if (requiredFields.Count == 0)
        {
            return new MotorYRequiredRatedParamFieldCoverageSnapshot
            {
                CanonicalCode = canonicalCode,
                CoveredRequiredRatedParamFieldCount = 0,
                MissingRequiredRatedParamFieldCount = 0,
                MissingRequiredRatedParamFields = Array.Empty<string>(),
                CoveredRequiredRatedParamFields = Array.Empty<string>(),
                RequiredRatedParamFieldCoverageRatio = 1d,
                RequiredRatedParamFieldCoveragePercentagePoints = 100,
                RatedParamsAvailable = ratedParams is not null,
                RequiredRatedParamFieldCoverageSummary = "no required rated param fields"
            };
        }

        if (ratedParams is null)
        {
            return Build(canonicalCode, requiredFields, Array.Empty<string>(), ratedParamsAvailable: false);
        }

        var coveredFields = requiredFields
            .Where(field => HasRatedParamField(ratedParams, field))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return Build(canonicalCode, requiredFields, coveredFields, ratedParamsAvailable: true);
    }

    private static MotorYRequiredRatedParamFieldCoverageSnapshot Build(
        string canonicalCode,
        IReadOnlyList<string> requiredFields,
        IReadOnlyList<string> coveredFields,
        bool ratedParamsAvailable)
    {
        var missingFields = requiredFields
            .Where(field => !coveredFields.Contains(field, StringComparer.Ordinal))
            .ToArray();
        var coveredRequiredRatedParamFields = coveredFields
            .Distinct(StringComparer.Ordinal)
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToArray();
        var ratio = requiredFields.Count == 0
            ? 1d
            : Math.Round((double)coveredRequiredRatedParamFields.Length / requiredFields.Count, 4, MidpointRounding.AwayFromZero);
        var percentagePoints = (int)Math.Round(ratio * 100d, MidpointRounding.AwayFromZero);

        return new MotorYRequiredRatedParamFieldCoverageSnapshot
        {
            CanonicalCode = canonicalCode,
            CoveredRequiredRatedParamFieldCount = coveredRequiredRatedParamFields.Length,
            MissingRequiredRatedParamFieldCount = missingFields.Length,
            MissingRequiredRatedParamFields = missingFields,
            CoveredRequiredRatedParamFields = coveredRequiredRatedParamFields,
            RequiredRatedParamFieldCoverageRatio = ratio,
            RequiredRatedParamFieldCoveragePercentagePoints = percentagePoints,
            RatedParamsAvailable = ratedParamsAvailable,
            RequiredRatedParamFieldCoverageSummary = $"rated param required fields covered {coveredRequiredRatedParamFields.Length}/{requiredFields.Count} ({percentagePoints}pp); missing: {(missingFields.Length == 0 ? "none" : string.Join(", ", missingFields))}"
        };
    }

    private static bool HasRatedParamField(MotorRatedParamsContract ratedParams, string fieldName)
    {
        return fieldName switch
        {
            "GB" => !string.IsNullOrWhiteSpace(ratedParams.StandardCode) || !string.IsNullOrWhiteSpace(ratedParams.StandardCodeRaw),
            "RatedPower" => ratedParams.RatedPower > 0 || ratedParams.RatedPowerRaw > 0,
            "PolePairs" => ratedParams.PolePairs > 0,
            "Connection" => !string.IsNullOrWhiteSpace(ratedParams.Connection) || !string.IsNullOrWhiteSpace(ratedParams.ConnectionRaw),
            "Duty" => !string.IsNullOrWhiteSpace(ratedParams.Duty) || !string.IsNullOrWhiteSpace(ratedParams.DutyRaw),
            _ => false
        };
    }
}

internal sealed class MotorYRequiredRatedParamFieldCoverageSnapshot
{
    public string CanonicalCode { get; init; } = string.Empty;
    public int CoveredRequiredRatedParamFieldCount { get; init; }
    public int MissingRequiredRatedParamFieldCount { get; init; }
    public IReadOnlyList<string> MissingRequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CoveredRequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public double RequiredRatedParamFieldCoverageRatio { get; init; }
    public int RequiredRatedParamFieldCoveragePercentagePoints { get; init; }
    public bool RatedParamsAvailable { get; init; }
    public string RequiredRatedParamFieldCoverageSummary { get; init; } = string.Empty;
}
