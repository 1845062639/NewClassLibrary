namespace StandardTestNext.Test.Domain.Records;

public sealed class TestRecordItemAggregate
{
    public Guid TestRecordItemId { get; set; } = Guid.NewGuid();
    public string ItemCode { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public string? Remark { get; set; }
    public bool IsValid { get; set; } = true;
    public List<RecordAttachment> Attachments { get; set; } = new();
}
