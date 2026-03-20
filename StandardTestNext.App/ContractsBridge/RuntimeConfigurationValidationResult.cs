namespace StandardTestNext.App.ContractsBridge;

public sealed class RuntimeConfigurationValidationResult
{
    public List<string> Warnings { get; } = new();

    public bool HasWarnings => Warnings.Count > 0;
}
