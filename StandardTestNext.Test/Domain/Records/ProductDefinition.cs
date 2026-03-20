namespace StandardTestNext.Test.Domain.Records;

public sealed class ProductDefinition
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public string ProductKind { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string RatedParamsJson { get; set; } = "{}";
    public string? Remark { get; set; }
    public bool IsValid { get; set; } = true;
}
