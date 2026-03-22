using System.Reflection;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public static class InProcAppQueryGatewayFactory
{
    public static ITestRecordQueryGateway CreateDefaultGateway()
    {
        return TestRecordQueryGatewayFactory.Create(TryCreateSeededGateway);
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
