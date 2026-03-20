namespace StandardTestNext.App.ContractsBridge;

public sealed class RuntimeConfigurationValidationResult
{
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();

    public bool HasWarnings => Warnings.Count > 0;
    public bool HasErrors => Errors.Count > 0;
}
