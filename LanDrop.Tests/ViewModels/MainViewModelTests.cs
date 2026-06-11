using LanDrop.ViewModels;
using Xunit;

namespace LanDrop.Tests.ViewModels;

[Collection("UsesNetworkPorts")]
public class MainViewModelTests : IDisposable
{
    private MainViewModel? _vm;

    public void Dispose()
    {
        _vm?.Dispose();
    }

    [Fact]
    public void Devices_InitiallyEmpty()
    {
        _vm = new MainViewModel();
        Assert.Empty(_vm.Devices);
    }

    [Fact]
    public void SendClipboardCommand_CanExecute_ReturnsFalse_WhenNoDeviceSelected()
    {
        _vm = new MainViewModel();
        Assert.False(_vm.SendClipboardCommand.CanExecute(null));
    }

    [Fact]
    public void LocalName_ReturnsMachineName()
    {
        _vm = new MainViewModel();
        Assert.Equal(System.Environment.MachineName, _vm.LocalName);
    }
}
