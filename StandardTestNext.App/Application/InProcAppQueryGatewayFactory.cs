using StandardTestNext.App.Application.Services;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.AppSide;

namespace StandardTestNext.App.Application;

public static class InProcAppQueryGatewayFactory
{
    public static DefaultQueryGatewayResolution ResolveDefaultGateway(AppQueryGatewayMode mode = AppQueryGatewayMode.Auto)
    {
        var gateway = mode switch
        {
            AppQueryGatewayMode.SeededInProc => TestRecordQueryGatewayFactory.Create(TryCreateSeededGateway),
            AppQueryGatewayMode.NullFallback => TestRecordQueryGatewayFactory.Create(),
            _ => TestRecordQueryGatewayFactory.Create(TryCreateSeededGateway)
        };

        return new DefaultQueryGatewayResolution
        {
            Gateway = gateway,
            ResolutionKind = TestRecordQueryGatewayFactory.IsNullGateway(gateway)
                ? DefaultQueryGatewayResolutionKind.NullFallback
                : DefaultQueryGatewayResolutionKind.SeededInProc
        };
    }

    private static ITestRecordQueryGateway? TryCreateSeededGateway()
    {
        return InProcAppQueryGatewaySeedFactory.CreateSeededGateway();
    }
}

public sealed class DefaultQueryGatewayResolution
{
    public ITestRecordQueryGateway Gateway { get; init; } = TestRecordQueryGatewayFactory.Create();
    public DefaultQueryGatewayResolutionKind ResolutionKind { get; init; }
}

public enum DefaultQueryGatewayResolutionKind
{
    SeededInProc,
    NullFallback
}
