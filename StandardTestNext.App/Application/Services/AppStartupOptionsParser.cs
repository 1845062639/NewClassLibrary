namespace StandardTestNext.App.Application.Services;

public static class AppStartupOptionsParser
{
    public static AppStartupOptions Parse(string[] args)
    {
        string? configPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                configPath = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
            {
                configPath = arg[("--config=".Length)..].Trim();
            }
        }

        var fileConfiguration = AppRuntimeConfigurationLoader.Load(configPath);
        var fileOptions = fileConfiguration.ToStartupOptions();
        var deviceId = fileOptions.DeviceId?.Trim();
        var productKind = fileOptions.ProductKind?.Trim();
        var samplingMode = fileOptions.SamplingMode?.Trim();
        var queryGatewayMode = fileOptions.QueryGatewayMode;
        var queryGatewaySqliteDbPath = fileOptions.QueryGatewaySqliteDbPath?.Trim();
        var messageBus = CloneMessageBusConfiguration(fileConfiguration.MessageBus);

        MessageBusRuntimeOverrides.ApplyEnvironmentOverrides(messageBus);

        var envDeviceId = Environment.GetEnvironmentVariable("STNEXT_APP_DEVICE_ID")?.Trim();
        if (!string.IsNullOrWhiteSpace(envDeviceId))
        {
            deviceId = envDeviceId;
        }

        var envProductKind = Environment.GetEnvironmentVariable("STNEXT_APP_PRODUCT_KIND")?.Trim();
        if (!string.IsNullOrWhiteSpace(envProductKind))
        {
            productKind = envProductKind;
        }

        var envSamplingMode = Environment.GetEnvironmentVariable("STNEXT_APP_SAMPLING_MODE")?.Trim();
        if (!string.IsNullOrWhiteSpace(envSamplingMode))
        {
            samplingMode = envSamplingMode;
        }

        var envQueryGatewayMode = Environment.GetEnvironmentVariable("STNEXT_APP_QUERY_GATEWAY")?.Trim();
        if (TryParseQueryGatewayMode(envQueryGatewayMode, out var parsedEnvQueryGatewayMode))
        {
            queryGatewayMode = parsedEnvQueryGatewayMode;
        }

        var envQueryGatewaySqliteDbPath = Environment.GetEnvironmentVariable("STNEXT_APP_QUERY_GATEWAY_SQLITE_DB")?.Trim();
        if (!string.IsNullOrWhiteSpace(envQueryGatewaySqliteDbPath))
        {
            queryGatewaySqliteDbPath = envQueryGatewaySqliteDbPath;
        }

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--device-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                deviceId = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--device-id=", StringComparison.OrdinalIgnoreCase))
            {
                deviceId = arg[("--device-id=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--product-kind", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                productKind = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--product-kind=", StringComparison.OrdinalIgnoreCase))
            {
                productKind = arg[("--product-kind=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--sampling-mode", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                samplingMode = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--sampling-mode=", StringComparison.OrdinalIgnoreCase))
            {
                samplingMode = arg[("--sampling-mode=".Length)..].Trim();
                continue;
            }

            if (string.Equals(arg, "--query-gateway", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                var value = args[++i].Trim();
                if (TryParseQueryGatewayMode(value, out var parsedQueryGatewayMode))
                {
                    queryGatewayMode = parsedQueryGatewayMode;
                }
                continue;
            }

            if (arg.StartsWith("--query-gateway=", StringComparison.OrdinalIgnoreCase))
            {
                var value = arg[("--query-gateway=".Length)..].Trim();
                if (TryParseQueryGatewayMode(value, out var parsedQueryGatewayMode))
                {
                    queryGatewayMode = parsedQueryGatewayMode;
                }
                continue;
            }

            if (string.Equals(arg, "--query-gateway-sqlite-db", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                queryGatewaySqliteDbPath = args[++i].Trim();
                continue;
            }

            if (arg.StartsWith("--query-gateway-sqlite-db=", StringComparison.OrdinalIgnoreCase))
            {
                queryGatewaySqliteDbPath = arg[("--query-gateway-sqlite-db=".Length)..].Trim();
                continue;
            }
        }

        MessageBusRuntimeOverrides.ApplyCommandLineOverrides(messageBus, args);

        return new AppStartupOptions
        {
            DeviceId = string.IsNullOrWhiteSpace(deviceId) ? "mock-motor-device" : deviceId,
            ProductKind = string.IsNullOrWhiteSpace(productKind) ? "Motor_Y" : productKind,
            SamplingMode = string.IsNullOrWhiteSpace(samplingMode) ? "single" : samplingMode,
            QueryGatewayMode = queryGatewayMode,
            QueryGatewaySqliteDbPath = string.IsNullOrWhiteSpace(queryGatewaySqliteDbPath) ? null : queryGatewaySqliteDbPath,
            MessageBus = messageBus
        };
    }

    public static bool TryParseQueryGatewayMode(string? value, out AppQueryGatewayMode mode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            mode = AppQueryGatewayMode.Auto;
            return false;
        }

        var normalized = value.Trim().Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        switch (normalized)
        {
            case "auto":
                mode = AppQueryGatewayMode.Auto;
                return true;
            case "seededinproc":
            case "seeded":
            case "inproc":
                mode = AppQueryGatewayMode.SeededInProc;
                return true;
            case "sqliteinproc":
            case "sqlite":
                mode = AppQueryGatewayMode.SqliteInProc;
                return true;
            case "nullfallback":
            case "null":
            case "none":
                mode = AppQueryGatewayMode.NullFallback;
                return true;
            default:
                mode = AppQueryGatewayMode.Auto;
                return false;
        }
    }

    private static MessageBusConfiguration CloneMessageBusConfiguration(MessageBusConfiguration configuration)
    {
        return new MessageBusConfiguration
        {
            Provider = configuration.Provider,
            Host = configuration.Host,
            Port = configuration.Port,
            ClientId = configuration.ClientId,
            TopicPrefix = configuration.TopicPrefix,
            Username = configuration.Username,
            Password = configuration.Password
        };
    }
}
