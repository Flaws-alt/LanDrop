using LanDrop.Models;
using LanDrop.Services;
using Xunit;

namespace LanDrop.Tests.Services;

[Collection("UsesNetworkPorts")]
public class DiscoveryServiceTests : IDisposable
{
    private DiscoveryService? _service;

    public void Dispose()
    {
        _service?.Dispose();
    }

    [Fact]
    public void GetLocalInfo_ReturnsMachineNameAndAssignedPort()
    {
        _service = new DiscoveryService();
        _service.Start();

        var info = _service.GetLocalInfo();

        Assert.Equal(System.Environment.MachineName, info.Name);
        Assert.True(info.Port > 0, "TCP port should be assigned");
        Assert.NotEmpty(info.Ip);
    }

    [Fact]
    public void DiscoveredDevices_InitiallyEmpty()
    {
        _service = new DiscoveryService();
        _service.Start();

        var devices = _service.GetDiscoveredDevices();

        Assert.Empty(devices);
    }

    [Fact]
    public void MessageReceived_Event_Fires_With_Deserialized_Message()
    {
        _service = new DiscoveryService();
        var received = false;
        DiscoveryMessage? lastMsg = null;
        _service.DeviceDiscovered += (s, msg) =>
        {
            received = true;
            lastMsg = msg;
        };

        var testMsg = new DiscoveryMessage
        {
            Type = "hello",
            Name = "TestPC",
            Ip = "127.0.0.1",
            Port = 12345
        };

        _service.SimulateReceivedMessage(testMsg);

        Assert.True(received);
        Assert.Equal("TestPC", lastMsg!.Name);
        Assert.Equal("127.0.0.1", lastMsg.Ip);
        Assert.Equal(12345, lastMsg.Port);
    }
}
