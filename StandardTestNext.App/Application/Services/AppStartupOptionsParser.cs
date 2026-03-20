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
        var deviceId = fileConfiguration.DeviceId?.Trim();
        var productKind = fileConfiguration.ProductKind?.Trim();
        var samplingMode = fileConfiguration.SamplingMode?.Trim();
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

        }

        MessageBusRuntimeOverrides.ApplyCommandLineOverrides(messageBus, args);

        return new AppStartupOptions
        {
            DeviceId = string.IsNullOrWhiteSpace(deviceId) ? "mock-motor-device" : deviceId,
            ProductKind = string.IsNullOrWhiteSpace(productKind) ? "Motor_Y" : productKind,
            SamplingMode = string.IsNullOrWhiteSpace(samplingMode) ? "single" : samplingMode,
            MessageBus = messageBus
        };
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
