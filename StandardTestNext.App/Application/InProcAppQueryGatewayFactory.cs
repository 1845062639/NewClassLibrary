using System.Reflection;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public static class InProcAppQueryGatewayFactory
{
    public static DefaultQueryGatewayResolution ResolveDefaultGateway()
    {
        var gateway = TestRecordQueryGatewayFactory.Create(TryCreateSeededGateway);
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
        var typeName = "StandardTestNext.Test.Application.AppSide.InProcAppQueryGatewaySeedFactory, StandardTestNext.Test";
        var factoryType = Type.GetType(typeName, throwOnError: false);
        var createMethod = factoryType?.GetMethod(
            "CreateSeededGateway",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        return createMethod?.Invoke(null, null) as ITestRecordQueryGateway;
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
