using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestCommandBuilder
{
    public TestCommandContract BuildStartCommand(string productKind, string sessionId)
    {
        return new TestCommandContract
        {
            ProductKind = productKind,
            SessionId = sessionId,
            CommandName = "StartNoLoadTest",
            Operator = "system-demo",
            IssuedAt = DateTimeOffset.Now,
            Parameters = new Dictionary<string, string>
            {
                ["SamplingIntervalMs"] = "1000",
                ["RecordPolicy"] = "KeyPointOnly"
            }
        };
    }
}
