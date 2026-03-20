using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.App.Application.Services;

public static class RuntimeConfigurationValidator
{
    public static RuntimeConfigurationValidationResult ValidateApp(AppStartupOptions options, MessageBusOptions messageBus)
    {
        var result = new RuntimeConfigurationValidationResult();

        if (!string.Equals(messageBus.Provider, "inmemory", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add($"Unsupported message bus provider '{messageBus.Provider}'. Current factory only supports inmemory and mqtt.");
        }

        if (string.IsNullOrWhiteSpace(options.DeviceId))
        {
            result.Warnings.Add("App deviceId is empty; startup will fall back to mock-motor-device.");
        }

        if (!string.Equals(options.SamplingMode, "single", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.SamplingMode, "burst", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add($"App samplingMode '{options.SamplingMode}' is not one of [single, burst].");
        }

        ValidateSharedMessageBus(result, messageBus);
        return result;
    }

    public static RuntimeConfigurationValidationResult ValidateSharedMessageBusOnly(MessageBusOptions messageBus)
    {
        var result = new RuntimeConfigurationValidationResult();
        ValidateSharedMessageBus(result, messageBus);
        return result;
    }

    private static void ValidateSharedMessageBus(RuntimeConfigurationValidationResult result, MessageBusOptions messageBus)
    {
        if (messageBus.Port is <= 0 or > 65535)
        {
            result.Errors.Add($"messageBus.port '{messageBus.Port}' is outside the valid TCP port range.");
        }

        if (string.IsNullOrWhiteSpace(messageBus.ClientId))
        {
            result.Errors.Add("messageBus.clientId is empty; set distinct client ids for App/Test.");
        }

        if (string.IsNullOrWhiteSpace(messageBus.TopicPrefix))
        {
            result.Errors.Add("messageBus.topicPrefix is empty; keep an explicit shared prefix such as 'stnext'.");
        }
    }
}
