namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyItemCodeNormalizer
{
    private static readonly Dictionary<string, string> AliasToCanonical = new(StringComparer.OrdinalIgnoreCase)
    {
        ["直流电阻测定"] = MotorYTestMethodCodes.DcResistance,
        ["陪试直流电阻测定"] = MotorYTestMethodCodes.DcResistance,

        ["空载特性试验"] = MotorYTestMethodCodes.NoLoad,
        ["空载试验"] = MotorYTestMethodCodes.NoLoad,
        ["空载试验（出厂）"] = MotorYTestMethodCodes.NoLoad,
        ["空载特性测量"] = MotorYTestMethodCodes.NoLoad,
        ["空载特性完全试验"] = MotorYTestMethodCodes.NoLoad,

        ["热试验"] = MotorYTestMethodCodes.HeatRun,
        ["热试验2"] = MotorYTestMethodCodes.HeatRun,
        ["温度计法热试验"] = MotorYTestMethodCodes.HeatRun,
        ["陪试热试验"] = MotorYTestMethodCodes.HeatRun,

        ["A法负载试验"] = MotorYTestMethodCodes.LoadA,
        ["B法负载试验"] = MotorYTestMethodCodes.LoadB,

        ["堵转特性试验"] = MotorYTestMethodCodes.LockedRotor,
        ["堵转试验"] = MotorYTestMethodCodes.LockedRotor,
        ["堵转试验（出厂）"] = MotorYTestMethodCodes.LockedRotor
    };

    private static readonly IReadOnlyDictionary<string, string[]> CanonicalToAliases = AliasToCanonical
        .GroupBy(x => x.Value, StringComparer.Ordinal)
        .ToDictionary(
            group => group.Key,
            group => group.Select(x => x.Key)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);

    public static string Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        var trimmed = code.Trim();
        return AliasToCanonical.TryGetValue(trimmed, out var canonical)
            ? canonical
            : trimmed;
    }

    public static IReadOnlyList<string> GetLegacyAliases(string? canonicalCode)
    {
        if (string.IsNullOrWhiteSpace(canonicalCode))
        {
            return Array.Empty<string>();
        }

        return CanonicalToAliases.TryGetValue(canonicalCode.Trim(), out var aliases)
            ? aliases
            : Array.Empty<string>();
    }

    public static bool IsMotorYCoreTrial(string? code)
    {
        var normalized = Normalize(code);
        return normalized is MotorYTestMethodCodes.DcResistance
            or MotorYTestMethodCodes.NoLoad
            or MotorYTestMethodCodes.HeatRun
            or MotorYTestMethodCodes.LoadA
            or MotorYTestMethodCodes.LoadB
            or MotorYTestMethodCodes.LockedRotor;
    }
}
