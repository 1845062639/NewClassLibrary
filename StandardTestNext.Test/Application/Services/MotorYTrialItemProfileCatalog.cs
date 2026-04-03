using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYTrialItemProfileCatalog
{
    public static IReadOnlyDictionary<string, MotorYTrialItemBuildProfile> BaselineProfiles { get; } =
        BuildBaselineProfiles();

    public static MotorYTrialItemBuildProfile GetRequiredBaseline(string canonicalCode)
    {
        if (!BaselineProfiles.TryGetValue(canonicalCode, out var profile))
        {
            throw new InvalidOperationException($"Motor_Y baseline build profile not found for canonical code '{canonicalCode}'.");
        }

        return profile;
    }

    public static void ApplyBaseline(TestRecordItemAggregate item, string canonicalCode)
    {
        var profile = GetRequiredBaseline(canonicalCode);
        item.MethodValue = profile.MethodValue;
        item.BuildProfile = profile;
    }

    private static IReadOnlyDictionary<string, MotorYTrialItemBuildProfile> BuildBaselineProfiles()
    {
        var codes = new[]
        {
            MotorYTestMethodCodes.DcResistance,
            MotorYTestMethodCodes.NoLoad,
            MotorYTestMethodCodes.HeatRun,
            MotorYTestMethodCodes.LoadA,
            MotorYTestMethodCodes.LoadB,
            MotorYTestMethodCodes.LockedRotor
        };

        var result = new Dictionary<string, MotorYTrialItemBuildProfile>(StringComparer.Ordinal);
        foreach (var canonicalCode in codes)
        {
            var baselineMethod = MotorYMethodProfileCatalog
                .GetKnownMethods(canonicalCode)
                .OrderBy(method => method)
                .Select(method => MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, method))
                .FirstOrDefault(route => route?.IsBaselineMethod == true);

            if (baselineMethod is null)
            {
                throw new InvalidOperationException($"Motor_Y baseline route not found for '{canonicalCode}'.");
            }

            result[canonicalCode] = MotorYTrialItemBuildProfile.FromRoute(baselineMethod);
        }

        return result;
    }
}
