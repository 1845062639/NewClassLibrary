namespace StandardTestNext.Test.RuntimeBridge;

public sealed class RuntimeConfigurationValidationResult
{
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();
    public List<string> Infos { get; } = new();

    public bool HasWarnings => Warnings.Count > 0;
    public bool HasErrors => Errors.Count > 0;
    public bool HasInfos => Infos.Count > 0;
}
