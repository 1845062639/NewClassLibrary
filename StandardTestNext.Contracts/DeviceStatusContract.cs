namespace StandardTestNext.Contracts;

public sealed class DeviceStatusContract
{
    public string DeviceId { get; set; } = string.Empty;
    public string ProductKind { get; set; } = string.Empty;
    public string ConnectionState { get; set; } = string.Empty;
    public string WorkState { get; set; } = string.Empty;
    public string? ActiveCommandName { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTimeOffset StatusTime { get; set; }
}
