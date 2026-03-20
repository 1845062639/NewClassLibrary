namespace StandardTestNext.App.Application.Services;

public sealed class RuntimeConfigurationValidationResult
{
    public List<string> Warnings { get; } = new();
    public bool HasWarnings => Warnings.Count > 0;
}
