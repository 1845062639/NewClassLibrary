using System.Text.Json;

namespace StandardTestNext.Test.Application.Services;

public static class TestRuntimeConfigurationLoader
{
    public const string DefaultConfigFileName = "appsettings.test.json";

    public static TestRuntimeConfiguration Load(string? configPath = null)
    {
        var resolvedPath = ResolveConfigPath(configPath);
        if (resolvedPath is null || !File.Exists(resolvedPath))
        {
            return new TestRuntimeConfiguration();
        }

        var json = File.ReadAllText(resolvedPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new TestRuntimeConfiguration();
        }

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        var configuration = JsonSerializer.Deserialize<TestRuntimeConfiguration>(json, options);
        return configuration ?? new TestRuntimeConfiguration();
    }

    private static string? ResolveConfigPath(string? configPath)
    {
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return Path.GetFullPath(configPath);
        }

        var baseDirectory = AppContext.BaseDirectory;
        var defaultPath = Path.Combine(baseDirectory, DefaultConfigFileName);
        if (File.Exists(defaultPath))
        {
            return defaultPath;
        }

        var cwdPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultConfigFileName);
        return File.Exists(cwdPath) ? cwdPath : defaultPath;
    }
}
