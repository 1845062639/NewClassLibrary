using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.App.Application.Services;

public static class RuntimeConfigurationConsoleReporter
{
    public static void ReportApp(AppStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[App.Config] deviceId={options.DeviceId}, productKind={options.ProductKind}, samplingMode={options.SamplingMode}");
        Console.WriteLine($"[App.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");
        ReportWarnings(validation);
    }


    private static void ReportWarnings(RuntimeConfigurationValidationResult validation)
    {
        if (!validation.HasWarnings)
        {
            Console.WriteLine("[Config.Validation] no warnings");
            return;
        }

        foreach (var warning in validation.Warnings)
        {
            Console.WriteLine($"[Config.Warning] {warning}");
        }
    }
}
