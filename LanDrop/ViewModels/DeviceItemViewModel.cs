using CommunityToolkit.Mvvm.ComponentModel;

namespace LanDrop.ViewModels;

public partial class DeviceItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _ip = string.Empty;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private string _deviceKey = string.Empty;

    public DeviceItemViewModel(string name, string ip, int port)
    {
        _name = name;
        _ip = ip;
        _port = port;
        _deviceKey = $"{ip}:{port}";
    }
}
