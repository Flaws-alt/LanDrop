using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LanDrop.Models;

namespace LanDrop.Services;

public class DiscoveryService : IDisposable
{
    public const int DiscoveryPort = 9527;
    public const int HeartbeatIntervalMs = 3000;
    public const int OfflineTimeoutMs = 10000;

    public event EventHandler<DiscoveryMessage>? DeviceDiscovered;
    public event EventHandler<DiscoveryMessage>? DeviceOffline;

    private readonly UdpClient _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private Task? _heartbeatTask;
    private Task? _cleanupTask;
    private readonly Dictionary<string, (DiscoveryMessage Info, DateTime LastSeen)> _devices = new();
    private readonly object _lock = new();

    private string _localIp = string.Empty;
    private int _tcpPort;

    public DiscoveryService()
    {
        _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, DiscoveryPort));
        _udpClient.EnableBroadcast = true;
    }

    public void Start()
    {
        _localIp = GetLocalIpAddress();
        _tcpPort = StartTcpListener();
        _cts = new CancellationTokenSource();

        _listenTask = Task.Run(() => ListenLoop(_cts.Token));
        _heartbeatTask = Task.Run(() => HeartbeatLoop(_cts.Token));
        _cleanupTask = Task.Run(() => CleanupLoop(_cts.Token));

        Broadcast("hello");
    }

    public void Stop()
    {
        Broadcast("bye");
        _cts?.Cancel();
        try { _listenTask?.Wait(2000); } catch { }
        try { _heartbeatTask?.Wait(2000); } catch { }
        try { _cleanupTask?.Wait(2000); } catch { }
    }

    public DeviceInfo GetLocalInfo()
    {
        return new DeviceInfo
        {
            Name = Environment.MachineName,
            Ip = _localIp,
            Port = _tcpPort
        };
    }

    public List<DeviceInfo> GetDiscoveredDevices()
    {
        lock (_lock)
        {
            return _devices.Values
                .Select(d => new DeviceInfo { Name = d.Info.Name, Ip = d.Info.Ip, Port = d.Info.Port })
                .ToList();
        }
    }

    public void SimulateReceivedMessage(DiscoveryMessage msg)
    {
        HandleMessage(msg);
    }

    private string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    private int StartTcpListener()
    {
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private void Broadcast(string type)
    {
        var msg = new DiscoveryMessage
        {
            Type = type,
            Name = Environment.MachineName,
            Ip = _localIp,
            Port = _tcpPort
        };
        var json = JsonSerializer.Serialize(msg);
        var data = Encoding.UTF8.GetBytes(json);
        _udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, DiscoveryPort));
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(ct);
                var json = Encoding.UTF8.GetString(result.Buffer);
                var msg = JsonSerializer.Deserialize<DiscoveryMessage>(json);
                if (msg != null && msg.Ip != _localIp)
                {
                    HandleMessage(msg);
                }
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
    }

    private async Task HeartbeatLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(HeartbeatIntervalMs, ct);
                Broadcast("heartbeat");
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task CleanupLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(OfflineTimeoutMs, ct);
                var now = DateTime.UtcNow;
                List<DiscoveryMessage>? offline = null;
                lock (_lock)
                {
                    var stale = _devices
                        .Where(kv => (now - kv.Value.LastSeen).TotalMilliseconds > OfflineTimeoutMs)
                        .ToList();
                    foreach (var kv in stale)
                    {
                        _devices.Remove(kv.Key);
                    }
                    offline = stale.Select(s => s.Value.Info).ToList();
                }
                if (offline != null && offline.Count > 0)
                {
                    foreach (var m in offline)
                        DeviceOffline?.Invoke(this, m);
                }
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private void HandleMessage(DiscoveryMessage msg)
    {
        var key = $"{msg.Ip}:{msg.Port}";
        bool isNew = false;
        lock (_lock)
        {
            if (!_devices.ContainsKey(key))
            {
                isNew = true;
            }
            _devices[key] = (msg, DateTime.UtcNow);
        }
        if (isNew)
        {
            DeviceDiscovered?.Invoke(this, msg);
        }
    }

    public void Dispose()
    {
        Stop();
        _udpClient?.Dispose();
        _cts?.Dispose();
    }
}

public class DeviceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public int Port { get; set; }
}
