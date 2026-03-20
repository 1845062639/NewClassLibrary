using System.Text.Json;

namespace StandardTestNext.App.Application.Services;

public static class AppRuntimeConfigurationLoader
{
    public const string DefaultConfigFileName = "appsettings.app.json";

    public static AppRuntimeConfiguration Load(string? configPath = null)
    {
        var resolvedPath = ResolveConfigPath(configPath);
        if (resolvedPath is null || !File.Exists(resolvedPath))
        {
            return new AppRuntimeConfiguration();
        }

        var json = File.ReadAllText(resolvedPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppRuntimeConfiguration();
        }

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        var configuration = JsonSerializer.Deserialize<AppRuntimeConfiguration>(json, options);
        return configuration ?? new AppRuntimeConfiguration();
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
