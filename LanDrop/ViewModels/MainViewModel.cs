using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanDrop.Models;
using LanDrop.Services;

namespace LanDrop.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly DiscoveryService _discovery;
    private readonly TransferService _transfer;
    private readonly ClipboardService _clipboard;
    private readonly AppSettings _settings;

    [ObservableProperty]
    private string _localName = string.Empty;

    [ObservableProperty]
    private string _localIp = string.Empty;

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private DeviceItemViewModel? _selectedDevice;

    public ObservableCollection<DeviceItemViewModel> Devices { get; } = new();

    public MainViewModel()
    {
        _settings = AppSettings.Load();
        _localName = _settings.DisplayName;
        _discovery = new DiscoveryService();
        _transfer = new TransferService();
        _clipboard = new ClipboardService();

        _discovery.DeviceDiscovered += OnDeviceDiscovered;
        _discovery.DeviceOffline += OnDeviceOffline;
        _transfer.FileReceived += OnFileReceived;
        _transfer.TextReceived += OnTextReceived;

        StartServices();
    }

    private void StartServices()
    {
        _discovery.Start();
        var localInfo = _discovery.GetLocalInfo();
        LocalIp = localInfo.Ip;
        _transfer.Start(localInfo.Port);
    }

    private void OnDeviceDiscovered(object? sender, DiscoveryMessage msg)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var key = $"{msg.Ip}:{msg.Port}";
            var existing = Devices.FirstOrDefault(d => d.DeviceKey == key);
            if (existing == null)
            {
                Devices.Add(new DeviceItemViewModel(msg.Name, msg.Ip, msg.Port));
                StatusText = $"发现设备: {msg.Name}";
            }
        });
    }

    private void OnDeviceOffline(object? sender, DiscoveryMessage msg)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var key = $"{msg.Ip}:{msg.Port}";
            var device = Devices.FirstOrDefault(d => d.DeviceKey == key);
            if (device != null)
            {
                Devices.Remove(device);
                StatusText = $"{device.Name} 已离线";
            }
        });
    }

    private void OnFileReceived(object? sender, TransferMetadata meta)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusText = $"收到文件: {meta.Name}";
        });
    }

    private void OnTextReceived(object? sender, string text)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var preview = text.Length > 30 ? text[..30] + "..." : text;
            StatusText = $"收到文本: {preview}";
        });
    }

    [RelayCommand]
    private void SendFile()
    {
        if (SelectedDevice == null)
        {
            StatusText = "请先选择目标设备";
            return;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择要发送的文件",
            Multiselect = true
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
            {
                var fileName = Path.GetFileName(file);
                StatusText = $"正在发送: {fileName}...";
                try
                {
                    TransferService.SendFileAsync(file, SelectedDevice.Ip, SelectedDevice.Port).GetAwaiter().GetResult();
                    StatusText = $"已发送: {fileName}";
                }
                catch (Exception ex)
                {
                    StatusText = $"发送失败: {ex.Message}";
                }
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSendClipboard))]
    private void SendClipboard()
    {
        if (SelectedDevice == null) return;

        if (_clipboard.HasText())
        {
            var text = _clipboard.GetText();
            StatusText = "正在发送剪贴板...";
            try
            {
                TransferService.SendTextAsync(text, SelectedDevice.Ip, SelectedDevice.Port).GetAwaiter().GetResult();
                StatusText = "剪贴板已发送";
            }
            catch (Exception ex)
            {
                StatusText = $"发送失败: {ex.Message}";
            }
        }
    }

    private bool CanSendClipboard()
    {
        return SelectedDevice != null && _clipboard.HasText();
    }

    partial void OnSelectedDeviceChanged(DeviceItemViewModel? value)
    {
        SendClipboardCommand.NotifyCanExecuteChanged();
    }

    public void HandleFileDrop(string[] files)
    {
        if (SelectedDevice == null)
        {
            StatusText = "请先将文件拖放到目标设备名称上";
            return;
        }

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            StatusText = $"正在发送: {fileName}...";
            try
            {
                TransferService.SendFileAsync(file, SelectedDevice.Ip, SelectedDevice.Port).GetAwaiter().GetResult();
                StatusText = $"已发送: {fileName}";
            }
            catch (Exception ex)
            {
                StatusText = $"发送失败: {ex.Message}";
            }
        }
    }

    public void RefreshClipboardState()
    {
        SendClipboardCommand.NotifyCanExecuteChanged();
    }

    public void Shutdown()
    {
        _discovery.Stop();
        _transfer.Stop();
    }

    public void Dispose()
    {
        Shutdown();
        _discovery.Dispose();
    }
}
