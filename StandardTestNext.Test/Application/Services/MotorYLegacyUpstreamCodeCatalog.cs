namespace StandardTestNext.Test.Application.Services;

internal static class MotorYLegacyUpstreamCodeCatalog
{
    public static IReadOnlyList<string> GetAliases(string? canonicalCode)
    {
        if (string.IsNullOrWhiteSpace(canonicalCode))
        {
            return Array.Empty<string>();
        }

        return MotorYLegacyItemCodeNormalizer.GetLegacyAliases(canonicalCode);
    }

    public static IReadOnlyList<MotorYLegacyUpstreamCodeDistributionSnapshot> BuildDistributions(
        string? canonicalCode,
        IEnumerable<string>? observedLegacyCodes)
    {
        var aliases = GetAliases(canonicalCode);
        if (aliases.Count == 0)
        {
            return Array.Empty<MotorYLegacyUpstreamCodeDistributionSnapshot>();
        }

        var counts = aliases.ToDictionary(alias => alias, _ => 0, StringComparer.Ordinal);
        foreach (var code in observedLegacyCodes ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            var trimmed = code.Trim();
            if (counts.ContainsKey(trimmed))
            {
                counts[trimmed]++;
            }
        }

        var total = counts.Values.Sum();
        return counts
            .Select(entry => new MotorYLegacyUpstreamCodeDistributionSnapshot
            {
                CanonicalCode = canonicalCode ?? string.Empty,
                LegacyCode = entry.Key,
                Count = entry.Value,
                Share = total <= 0
                    ? 0d
                    : Math.Round((double)entry.Value / total, 4, MidpointRounding.AwayFromZero)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();
    }
}
