using StandardTestNext.App.Application.Services;

namespace StandardTestNext.App.Application;

public static class AppQueryGatewaySmokeTests
{
    public static void Run()
    {
        ShouldParseQueryGatewayModeFromCliAndEnvironment();
        ShouldResolveDefaultGatewayForRequestedModes();
    }

    private static void ShouldParseQueryGatewayModeFromCliAndEnvironment()
    {
        const string envKey = "STNEXT_APP_QUERY_GATEWAY";
        var original = Environment.GetEnvironmentVariable(envKey);

        try
        {
            Environment.SetEnvironmentVariable(envKey, "null");
            var fromEnvironment = AppStartupOptionsParser.Parse(Array.Empty<string>());
            if (fromEnvironment.QueryGatewayMode != AppQueryGatewayMode.NullFallback)
            {
                throw new InvalidOperationException("App query gateway env override smoke test failed.");
            }

            var fromCli = AppStartupOptionsParser.Parse(new[] { "--query-gateway=seeded-inproc" });
            if (fromCli.QueryGatewayMode != AppQueryGatewayMode.SeededInProc)
            {
                throw new InvalidOperationException("App query gateway CLI override smoke test failed.");
            }

            var fromCliAlias = AppStartupOptionsParser.Parse(new[] { "--query-gateway", "none" });
            if (fromCliAlias.QueryGatewayMode != AppQueryGatewayMode.NullFallback)
            {
                throw new InvalidOperationException("App query gateway CLI alias smoke test failed.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(envKey, original);
        }
    }

    private static void ShouldResolveDefaultGatewayForRequestedModes()
    {
        var nullFallback = InProcAppQueryGatewayFactory.ResolveDefaultGateway(AppQueryGatewayMode.NullFallback);
        if (nullFallback.ResolutionKind != DefaultQueryGatewayResolutionKind.NullFallback)
        {
            throw new InvalidOperationException("App query gateway null-fallback resolution smoke test failed.");
        }

        var seeded = InProcAppQueryGatewayFactory.ResolveDefaultGateway(AppQueryGatewayMode.SeededInProc);
        if (seeded.ResolutionKind != DefaultQueryGatewayResolutionKind.SeededInProc)
        {
            throw new InvalidOperationException("App query gateway seeded-inproc resolution smoke test failed.");
        }

        var auto = InProcAppQueryGatewayFactory.ResolveDefaultGateway(AppQueryGatewayMode.Auto);
        if (auto.ResolutionKind is not (DefaultQueryGatewayResolutionKind.SeededInProc or DefaultQueryGatewayResolutionKind.NullFallback))
        {
            throw new InvalidOperationException("App query gateway auto resolution smoke test failed.");
        }
    }
}
