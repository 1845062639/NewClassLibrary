using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRuntimeOrchestrator
{
    private readonly IMessagePublisher _publisher;
    private readonly TestCommandBuilder _commandBuilder;

    public TestRuntimeOrchestrator(IMessagePublisher publisher, TestCommandBuilder commandBuilder)
    {
        _publisher = publisher;
        _commandBuilder = commandBuilder;
    }

    public TestCommandContract StartDemoSession(string productKind)
    {
        var command = _commandBuilder.BuildStartCommand(productKind, $"session-{DateTimeOffset.Now:yyyyMMddHHmmss}");
        _publisher.Publish(ContractTopics.TestCommand, command);
        return command;
    }
}
