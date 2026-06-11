using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using LanDrop.Models;

namespace LanDrop.Services;

public class TransferService
{
    public event EventHandler<TransferMetadata>? FileReceived;
    public event EventHandler<string>? TextReceived;

    private TcpListener? _listener;
    private Task? _listenTask;
    private bool _running;

    public int ListenPort => _listener != null
        ? ((IPEndPoint)_listener.LocalEndpoint).Port
        : 0;

    public void Start(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _running = true;
        _listenTask = Task.Run(ListenLoop);
    }

    public void Stop()
    {
        _running = false;
        try { _listener?.Stop(); } catch { }
        try { _listenTask?.Wait(2000); } catch { }
    }

    public static async Task SendFileAsync(string filePath, string targetIp, int targetPort)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(targetIp, targetPort);
        using var stream = client.GetStream();

        var fileInfo = new FileInfo(filePath);
        var meta = new TransferMetadata
        {
            Type = "file",
            Name = fileInfo.Name,
            Size = fileInfo.Length
        };
        var metaJson = JsonSerializer.Serialize(meta);
        var metaBytes = Encoding.UTF8.GetBytes(metaJson);

        var lengthBytes = BitConverter.GetBytes(metaBytes.Length);
        await stream.WriteAsync(lengthBytes, 0, 4);
        await stream.WriteAsync(metaBytes, 0, metaBytes.Length);

        using var fileStream = File.OpenRead(filePath);
        await fileStream.CopyToAsync(stream);
    }

    public static async Task SendTextAsync(string text, string targetIp, int targetPort)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(targetIp, targetPort);
        using var stream = client.GetStream();

        var meta = new TransferMetadata
        {
            Type = "text",
            Data = text
        };
        var metaJson = JsonSerializer.Serialize(meta);
        var metaBytes = Encoding.UTF8.GetBytes(metaJson);

        var lengthBytes = BitConverter.GetBytes(metaBytes.Length);
        await stream.WriteAsync(lengthBytes, 0, 4);
        await stream.WriteAsync(metaBytes, 0, metaBytes.Length);
    }

    private async Task ListenLoop()
    {
        while (_running)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (ObjectDisposedException) { break; }
            catch (InvalidOperationException) { break; }
            catch { }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var lenBuf = new byte[4];
                var totalRead = 0;
                while (totalRead < 4)
                {
                    var read = await stream.ReadAsync(lenBuf.AsMemory(totalRead, 4 - totalRead));
                    if (read == 0) return;
                    totalRead += read;
                }
                var metaLen = BitConverter.ToInt32(lenBuf, 0);

                var metaBuf = new byte[metaLen];
                var offset = 0;
                while (offset < metaLen)
                {
                    var r = await stream.ReadAsync(metaBuf.AsMemory(offset, metaLen - offset));
                    if (r == 0) return;
                    offset += r;
                }
                var metaJson = Encoding.UTF8.GetString(metaBuf);
                var meta = JsonSerializer.Deserialize<TransferMetadata>(metaJson)!;

                if (meta.Type == "file")
                {
                    var receiveDir = AppSettings.Load().ReceiveDirectory;
                    Directory.CreateDirectory(receiveDir);
                    var filePath = GetUniqueFilePath(receiveDir, meta.Name);
                    using var fileStream = File.Create(filePath);
                    await stream.CopyToAsync(fileStream);
                    FileReceived?.Invoke(this, meta);
                }
                else if (meta.Type == "text" && !string.IsNullOrEmpty(meta.Data))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Clipboard.SetText(meta.Data);
                    });
                    TextReceived?.Invoke(this, meta.Data);
                }
            }
        }
        catch { }
    }

    private static string GetUniqueFilePath(string dir, string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var path = Path.Combine(dir, fileName);
        var counter = 1;

        while (File.Exists(path))
        {
            path = Path.Combine(dir, $"{baseName}_{counter}{ext}");
            counter++;
        }

        return path;
    }
}
