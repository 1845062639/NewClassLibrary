namespace StandardTestNext.Test.Domain.Records;

using StandardTestNext.Test.Application.Services;

public sealed class TestRecordItemAggregate
{
    public Guid TestRecordItemId { get; set; } = Guid.NewGuid();
    public string ItemCode { get; set; } = string.Empty;
    public string MethodCode { get; set; } = string.Empty;
    public int? MethodValue { get; set; }
    public MotorYTrialItemBuildProfile? BuildProfile { get; set; }
    public string DataJson { get; set; } = "{}";
    public string? Remark { get; set; }
    public bool IsValid { get; set; } = true;
    public List<RecordAttachment> Attachments { get; set; } = new();
}
