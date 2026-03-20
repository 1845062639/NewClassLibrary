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
            result.Errors.Add($"Test persistenceMode '{options.PersistenceMode}' is not one of [memory, sqlite].");
        }

        if (string.Equals(options.PersistenceMode, "sqlite", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(options.SQLiteDbPath))
        {
            result.Warnings.Add("Test persistenceMode=sqlite without explicit sqliteDbPath; bootstrap will use SQLiteTestPersistence.DefaultDbPath.");
        }

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

        if (!string.Equals(messageBus.Provider, "inmemory", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add($"Unsupported message bus provider '{messageBus.Provider}'. Current factory only supports inmemory and mqtt.");
        }

        return result;
    }

    public static void ReportTest(TestStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[Test.Config] persistenceMode={options.PersistenceMode}, sqliteDbPath={options.SQLiteDbPath ?? "<default>"}");
        Console.WriteLine($"[Test.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}");

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

    public static void ThrowIfInvalid(RuntimeConfigurationValidationResult validation)
    {
        if (!validation.HasErrors)
        {
            return;
        }

        throw new InvalidOperationException($"Invalid test runtime configuration: {string.Join(" | ", validation.Errors)}");
    }
}
