namespace StandardTestNext.App.ContractsBridge;

public sealed class ConnectivityProbeResult
{
    public bool Success { get; init; }
    public string Status { get; init; } = "unknown";
    public string? Detail { get; init; }

    public string ToDisplayText(string endpoint)
    {
        return string.IsNullOrWhiteSpace(Detail)
            ? $"{endpoint} self-check status={Status}"
            : $"{endpoint} self-check status={Status} ({Detail})";
    }
}
