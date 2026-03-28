namespace StandardTestNext.Test.Application.Services;

public sealed class StpDbMotorRatedParamsValueDistributionSnapshot
{
    public string FieldName { get; init; } = string.Empty;
    public string RawValue { get; init; } = string.Empty;
    public int Count { get; init; }
    public string? NormalizedValue { get; init; }
}
