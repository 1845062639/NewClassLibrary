using StandardTestNext.App.Application.Services;

namespace StandardTestNext.App.Application;

public static class AppQueryGatewayResolutionSmokeTests
{
    public static void Run()
    {
        ShouldResolveSeededGatewayWhenExplicitlyRequested();
        ShouldResolveNullGatewayWhenExplicitlyRequested();
        ShouldParseQueryGatewayAliases();
        ShouldApplyEnvironmentQueryGatewayOverride();
        ShouldLetCommandLineQueryGatewayOverrideEnvironment();
    }

    private static void ShouldResolveSeededGatewayWhenExplicitlyRequested()
    {
        var resolution = InProcAppQueryGatewayFactory.ResolveDefaultGateway(AppQueryGatewayMode.SeededInProc);
        if (resolution.ResolutionKind != DefaultQueryGatewayResolutionKind.SeededInProc)
        {
            throw new InvalidOperationException("Explicit seeded-inproc mode should resolve to seeded gateway.");
        }

        var list = resolution.Gateway.ListRecentAsync(5).GetAwaiter().GetResult();
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Explicit seeded-inproc mode returned an empty recent record list.");
        }
    }

    private static void ShouldResolveNullGatewayWhenExplicitlyRequested()
    {
        var resolution = InProcAppQueryGatewayFactory.ResolveDefaultGateway(AppQueryGatewayMode.NullFallback);
        if (resolution.ResolutionKind != DefaultQueryGatewayResolutionKind.NullFallback)
        {
            throw new InvalidOperationException("Explicit null-fallback mode should resolve to null gateway.");
        }

        var list = resolution.Gateway.ListRecentAsync(5).GetAwaiter().GetResult();
        if (list.Count != 0)
        {
            throw new InvalidOperationException("Explicit null-fallback mode should not return seeded records.");
        }
    }

    private static void ShouldParseQueryGatewayAliases()
    {
        var seededVariants = new[]
        {
            new[] { "--query-gateway", "seeded-inproc" },
            new[] { "--query-gateway=seeded" },
            new[] { "--query-gateway=inproc" }
        };

        foreach (var args in seededVariants)
        {
            var options = AppStartupOptionsParser.Parse(args);
            if (options.QueryGatewayMode != AppQueryGatewayMode.SeededInProc)
            {
                throw new InvalidOperationException("Seeded query gateway alias parsing failed.");
            }
        }

        var nullVariants = new[]
        {
            new[] { "--query-gateway", "null-fallback" },
            new[] { "--query-gateway=null" },
            new[] { "--query-gateway=none" }
        };

        foreach (var args in nullVariants)
        {
            var options = AppStartupOptionsParser.Parse(args);
            if (options.QueryGatewayMode != AppQueryGatewayMode.NullFallback)
            {
                throw new InvalidOperationException("Null query gateway alias parsing failed.");
            }
        }
    }

    private static void ShouldApplyEnvironmentQueryGatewayOverride()
    {
        const string variableName = "STNEXT_APP_QUERY_GATEWAY";
        var original = Environment.GetEnvironmentVariable(variableName);

        try
        {
            Environment.SetEnvironmentVariable(variableName, "seeded");
            var options = AppStartupOptionsParser.Parse(Array.Empty<string>());
            if (options.QueryGatewayMode != AppQueryGatewayMode.SeededInProc)
            {
                throw new InvalidOperationException("Environment query gateway override should resolve to seeded-inproc mode.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, original);
        }
    }

    private static void ShouldLetCommandLineQueryGatewayOverrideEnvironment()
    {
        const string variableName = "STNEXT_APP_QUERY_GATEWAY";
        var original = Environment.GetEnvironmentVariable(variableName);

        try
        {
            Environment.SetEnvironmentVariable(variableName, "seeded");
            var options = AppStartupOptionsParser.Parse(new[] { "--query-gateway=null" });
            if (options.QueryGatewayMode != AppQueryGatewayMode.NullFallback)
            {
                throw new InvalidOperationException("Command-line query gateway mode should override environment configuration.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, original);
        }
    }
}
