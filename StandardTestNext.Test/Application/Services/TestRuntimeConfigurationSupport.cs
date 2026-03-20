using StandardTestNext.App.ContractsBridge;

namespace StandardTestNext.Test.Application.Services;

public static class TestRuntimeConfigurationSupport
{
    public static RuntimeConfigurationValidationResult ValidateTest(TestStartupOptions options, MessageBusOptions messageBus)
    {
        var result = new RuntimeConfigurationValidationResult();

        if (!string.Equals(options.PersistenceMode, "memory", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.PersistenceMode, "sqlite", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add($"Test persistenceMode '{options.PersistenceMode}' is not one of [memory, sqlite]; bootstrap currently falls back to memory path unless sqlite is matched explicitly.");
        }

        if (string.Equals(options.PersistenceMode, "sqlite", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(options.SQLiteDbPath))
        {
            result.Warnings.Add("Test persistenceMode=sqlite without explicit sqliteDbPath; bootstrap will use SQLiteTestPersistence.DefaultDbPath.");
        }

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

        if (!string.Equals(messageBus.Provider, "inmemory", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add($"Unsupported message bus provider '{messageBus.Provider}'. Current factory only supports inmemory.");
        }
        else if (string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            result.Warnings.Add("messageBus.provider=mqtt is recognized in configuration, but MessageBusFactory has not implemented MQTT yet; startup will currently fail unless provider is switched back to inmemory.");
        }

        return result;
    }

    public static void ReportTest(TestStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[Test.Config] persistenceMode={options.PersistenceMode}, sqliteDbPath={options.SQLiteDbPath ?? "<default>"}");
        Console.WriteLine($"[Test.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");

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
