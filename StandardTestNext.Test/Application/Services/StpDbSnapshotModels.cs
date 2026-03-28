using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class StpDbProductTypeSnapshot
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string RatedParamsJson { get; init; } = "{}";
    public MotorRatedParamsContract? RatedParams { get; init; }
    public int? Category { get; init; }
    public string? Manufacturer { get; init; }
    public string? Remark { get; init; }
    public bool IsValid { get; init; }
    public string? CreateTimeRaw { get; init; }
    public string? CreateBy { get; init; }
    public string? UpdateTimeRaw { get; init; }
    public string? UpdateBy { get; init; }
}

public sealed class StpDbTestRecordSnapshot
{
    public string Id { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? SerialNumber { get; init; }
    public string? TestProductTypeId { get; init; }
    public string? AccompanyProductTypeId { get; init; }
    public int? Kind { get; init; }
    public string? OwnDepart { get; init; }
    public string? TestDepartId { get; init; }
    public string? TesterId { get; init; }
    public string? Remark { get; init; }
    public string? TestTimeRaw { get; init; }
    public bool IsValid { get; init; }
    public string? CreateTimeRaw { get; init; }
    public string? CreateBy { get; init; }
    public string? UpdateTimeRaw { get; init; }
    public string? UpdateBy { get; init; }
    public IReadOnlyList<StpDbFileAttachmentSnapshot> Attachments { get; init; } = Array.Empty<StpDbFileAttachmentSnapshot>();
}

public sealed class StpDbTestRecordItemSnapshot
{
    public string Id { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int? Method { get; init; }
    public string? CanonicalCode { get; init; }
    public string MethodKey { get; init; } = string.Empty;
    public string? MethodProfileKey { get; init; }
    public string? LegacyAlgorithmEntry { get; init; }
    public bool IsBaselineMethod { get; init; }
    public string DataJson { get; init; } = "{}";
    public string? Remark { get; init; }
    public string? TestRecordId { get; init; }
    public bool IsValid { get; init; }
    public string? CreateTimeRaw { get; init; }
    public string? CreateBy { get; init; }
    public string? UpdateTimeRaw { get; init; }
    public string? UpdateBy { get; init; }
    public IReadOnlyList<StpDbFileAttachmentSnapshot> Attachments { get; init; } = Array.Empty<StpDbFileAttachmentSnapshot>();
}

public sealed class StpDbFileAttachmentSnapshot
{
    public string Id { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileExt { get; init; } = string.Empty;
    public string? Path { get; init; }
    public long Length { get; init; }
    public string? UploadTimeRaw { get; init; }
    public string? SaveMode { get; init; }
    public string? ExtraInfo { get; init; }
    public string? HandlerInfo { get; init; }
}

public sealed class StpDbMotorYRecordSnapshot
{
    public StpDbTestRecordSnapshot Record { get; init; } = new();
    public StpDbProductTypeSnapshot? ProductType { get; init; }
    public IReadOnlyList<StpDbTestRecordItemSnapshot> Items { get; init; } = Array.Empty<StpDbTestRecordItemSnapshot>();
}
