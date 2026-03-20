using StandardTestNext.App.ContractsBridge;
using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.App.Application.Services;

public static class RuntimeConfigurationConsoleReporter
{
    public static void ReportApp(AppStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[App.Config] deviceId={options.DeviceId}, productKind={options.ProductKind}, samplingMode={options.SamplingMode}");
        Console.WriteLine($"[App.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");
        ReportWarnings(validation);
    }

    public static void ReportTest(TestStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[Test.Config] persistenceMode={options.PersistenceMode}, sqliteDbPath={options.SQLiteDbPath ?? "<default>"}");
        Console.WriteLine($"[Test.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");
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
