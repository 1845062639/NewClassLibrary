using StandardTestNext.Contracts;
using System.Net.Sockets;
using System.Security.Authentication;

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

        if (string.Equals(options.PersistenceMode, "sqlite", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(options.SQLiteDbPath))
        {
            ValidateSqlitePath(result, options.SQLiteDbPath!);
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

        if (messageBus.PublishTimeoutSeconds <= 0)
        {
            result.Errors.Add($"messageBus.publishTimeoutSeconds '{messageBus.PublishTimeoutSeconds}' must be > 0.");
        }

        if (messageBus.SubscribeTimeoutSeconds <= 0)
        {
            result.Errors.Add($"messageBus.subscribeTimeoutSeconds '{messageBus.SubscribeTimeoutSeconds}' must be > 0.");
        }

        if (string.Equals(messageBus.Provider, "mqtt", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(messageBus.Host))
            {
                result.Errors.Add("messageBus.host is empty while provider=mqtt.");
            }
            else
            {
                var probe = ProbeEndpoint(messageBus.Host, messageBus.Port ?? 1883, TimeSpan.FromMilliseconds(800));
                if (probe.Success)
                {
                    result.Infos.Add(probe.ToDisplayText($"messageBus endpoint {messageBus.Host}:{messageBus.Port ?? 1883}"));
                }
                else
                {
                    result.Warnings.Add(probe.ToDisplayText($"messageBus endpoint {messageBus.Host}:{messageBus.Port ?? 1883}"));
                }
            }
        }

        return result;
    }

    public static void ReportTest(TestStartupOptions options, MessageBusOptions messageBus, RuntimeConfigurationValidationResult validation)
    {
        Console.WriteLine($"[Test.Config] persistenceMode={options.PersistenceMode}, sqliteDbPath={options.SQLiteDbPath ?? "<default>"}");
        Console.WriteLine($"[Test.Config] messageBus provider={messageBus.Provider}, host={messageBus.Host ?? "<null>"}, port={messageBus.Port?.ToString() ?? "<null>"}, clientId={messageBus.ClientId ?? "<null>"}, topicPrefix={messageBus.TopicPrefix ?? "<null>"}, publishTimeoutSeconds={messageBus.PublishTimeoutSeconds}, subscribeTimeoutSeconds={messageBus.SubscribeTimeoutSeconds}");

        foreach (var error in validation.Errors)
        {
            Console.WriteLine($"[Config.Error] {error}");
        }

        foreach (var warning in validation.Warnings)
        {
            Console.WriteLine($"[Config.Warning] {warning}");
        }

        foreach (var info in validation.Infos)
        {
            Console.WriteLine($"[Config.Info] {info}");
        }

        if (!validation.HasErrors && !validation.HasWarnings && !validation.HasInfos)
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

    private static void ValidateSqlitePath(RuntimeConfigurationValidationResult result, string sqliteDbPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(sqliteDbPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                result.Errors.Add($"sqliteDbPath '{sqliteDbPath}' does not resolve to a valid directory.");
                return;
            }

            Directory.CreateDirectory(directory);
            var probeFile = Path.Combine(directory, $".write-probe-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFile, "probe");
            File.Delete(probeFile);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"sqliteDbPath '{sqliteDbPath}' is not writable: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static ConnectivityProbeResult ProbeEndpoint(string host, int port, TimeSpan timeout)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            if (!connectTask.Wait(timeout))
            {
                return new ConnectivityProbeResult
                {
                    Success = false,
                    Status = "timeout",
                    Detail = $"no TCP handshake within {timeout.TotalMilliseconds:0}ms"
                };
            }

            return client.Connected
                ? new ConnectivityProbeResult
                {
                    Success = true,
                    Status = "reachable",
                    Detail = "tcp handshake succeeded"
                }
                : new ConnectivityProbeResult
                {
                    Success = false,
                    Status = "disconnected",
                    Detail = "connect completed but socket is not connected"
                };
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            return new ConnectivityProbeResult
            {
                Success = false,
                Status = "connection-refused",
                Detail = ex.Message
            };
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostNotFound || ex.SocketErrorCode == SocketError.NoData)
        {
            return new ConnectivityProbeResult
            {
                Success = false,
                Status = "dns-failed",
                Detail = ex.Message
            };
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            return new ConnectivityProbeResult
            {
                Success = false,
                Status = "timeout",
                Detail = ex.Message
            };
        }
        catch (AuthenticationException ex)
        {
            return new ConnectivityProbeResult
            {
                Success = false,
                Status = "auth-failed",
                Detail = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new ConnectivityProbeResult
            {
                Success = false,
                Status = "probe-failed",
                Detail = ex.GetType().Name + ": " + ex.Message
            };
        }
    }
}
