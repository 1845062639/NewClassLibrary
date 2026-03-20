namespace StandardTestNext.Contracts;

public sealed class TestCommandContract
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
    public string ProductKind { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string CommandName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
