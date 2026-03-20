using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.App.Application.Services;

public static class RuntimeConfigurationConsoleReporter
{
    public static void ReportApp(AppStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[App.Config] deviceId={options.DeviceId}, productKind={options.ProductKind}, samplingMode={options.SamplingMode}");
        Console.WriteLine($"[App.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");
        ReportValidation(validation);
    }

    public static void ThrowIfInvalid(RuntimeConfigurationValidationResult validation)
    {
        if (!validation.HasErrors)
        {
            return;
        }

        throw new InvalidOperationException($"Invalid app runtime configuration: {string.Join(" | ", validation.Errors)}");
    }

    private static void ReportValidation(RuntimeConfigurationValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            Console.WriteLine($"[Config.Error] {error}");
        }

        foreach (var warning in validation.Warnings)
        {
            Console.WriteLine($"[Config.Warning] {warning}");
        }

        if (!validation.HasErrors && !validation.HasWarnings)
        {
            Console.WriteLine("[Config.Validation] no warnings");
        }
    }
}
