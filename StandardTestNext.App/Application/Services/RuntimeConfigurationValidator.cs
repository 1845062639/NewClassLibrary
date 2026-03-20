using StandardTestNext.App.ContractsBridge;
using System.Net.Sockets;
using System.Security.Authentication;

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
