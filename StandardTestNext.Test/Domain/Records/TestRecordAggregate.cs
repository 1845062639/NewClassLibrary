namespace StandardTestNext.Test.Domain.Records;

public sealed class TestRecordAggregate
{
    public Guid TestRecordId { get; set; } = Guid.NewGuid();
    public string RecordCode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string TestKindCode { get; set; } = string.Empty;
    public string? OwnDepartment { get; set; }
    public string? TestDepartment { get; set; }
    public string? Tester { get; set; }
    public string? Remark { get; set; }
    public DateTimeOffset TestTime { get; set; }
    public bool IsValid { get; set; } = true;
    public ProductDefinition? TestProduct { get; set; }
    public ProductDefinition? AccompanyProduct { get; set; }
    public List<RecordAttachment> Attachments { get; } = new();
    public List<TestRecordItemAggregate> Items { get; } = new();
}
