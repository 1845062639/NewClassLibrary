using System.Reflection;
using StandardTestNext.Contracts;

namespace StandardTestNext.App.Application;

public static class InProcAppQueryGatewayFactory
{
    public static ITestRecordQueryGateway CreateSeededGateway()
    {
        var typeName = "StandardTestNext.Test.Application.AppSide.InProcAppQueryGatewaySeedFactory, StandardTestNext.Test";
        var factoryType = Type.GetType(typeName, throwOnError: false);
        var createMethod = factoryType?.GetMethod(
            "CreateSeededGateway",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (createMethod?.Invoke(null, null) is ITestRecordQueryGateway gateway)
        {
            return gateway;
        }

        throw new InvalidOperationException(
            $"Unable to create seeded test record query gateway via '{typeName}'.");
    }
}
