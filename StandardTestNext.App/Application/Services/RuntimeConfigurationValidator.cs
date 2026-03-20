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
            result.Warnings.Add($"Unsupported message bus provider '{messageBus.Provider}'. Current factory only supports inmemory.");
        }
        else if (string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add("messageBus.provider=mqtt is recognized in configuration, but MessageBusFactory has not implemented MQTT yet; startup will currently fail unless provider is switched back to inmemory.");
        }

        if (string.IsNullOrWhiteSpace(options.DeviceId))
        {
            result.Warnings.Add("App deviceId is empty; startup will fall back to mock-motor-device.");
        }

        if (!string.Equals(options.SamplingMode, "single", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.SamplingMode, "burst", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add($"App samplingMode '{options.SamplingMode}' is not one of [single, burst]; bootstrap currently normalizes unknown values but still runs.");
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
            result.Warnings.Add($"messageBus.port '{messageBus.Port}' is outside the valid TCP port range; current runtime will ignore it for inmemory, but MQTT should reject it later.");
        }

        if (string.IsNullOrWhiteSpace(messageBus.ClientId))
        {
            result.Warnings.Add("messageBus.clientId is empty; recommended to set distinct client ids for App/Test before MQTT lands.");
        }

        if (string.IsNullOrWhiteSpace(messageBus.TopicPrefix))
        {
            result.Warnings.Add("messageBus.topicPrefix is empty; recommended to keep an explicit shared prefix such as 'stnext'.");
        }
    }
}
