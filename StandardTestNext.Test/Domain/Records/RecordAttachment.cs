namespace StandardTestNext.Test.Domain.Records;

public sealed class RecordAttachment
{
    public Guid AttachmentId { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FileExt { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long Length { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public string? ExtraInfoJson { get; set; }
}
