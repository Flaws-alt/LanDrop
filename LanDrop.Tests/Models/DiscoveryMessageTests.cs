using System.Text.Json;
using LanDrop.Models;
using Xunit;

namespace LanDrop.Tests.Models;

public class DiscoveryMessageTests
{
    [Fact]
    public void Serialize_Hello_ProducesCorrectJson()
    {
        var msg = new DiscoveryMessage
        {
            Type = "hello",
            Name = "PC-A",
            Ip = "192.168.1.100",
            Port = 51234
        };

        var json = JsonSerializer.Serialize(msg);

        Assert.Contains("\"type\":\"hello\"", json);
        Assert.Contains("\"name\":\"PC-A\"", json);
        Assert.Contains("\"ip\":\"192.168.1.100\"", json);
        Assert.Contains("\"port\":51234", json);
    }

    [Fact]
    public void Deserialize_Hello_RestoresFields()
    {
        var json = "{\"type\":\"hello\",\"name\":\"PC-A\",\"ip\":\"192.168.1.100\",\"port\":51234}";

        var msg = JsonSerializer.Deserialize<DiscoveryMessage>(json)!;

        Assert.Equal("hello", msg.Type);
        Assert.Equal("PC-A", msg.Name);
        Assert.Equal("192.168.1.100", msg.Ip);
        Assert.Equal(51234, msg.Port);
    }

    [Fact]
    public void Deserialize_Heartbeat_RestoresFields()
    {
        var json = "{\"type\":\"heartbeat\",\"name\":\"PC-B\",\"ip\":\"10.0.0.5\",\"port\":9999}";

        var msg = JsonSerializer.Deserialize<DiscoveryMessage>(json)!;

        Assert.Equal("heartbeat", msg.Type);
        Assert.Equal("PC-B", msg.Name);
        Assert.Equal("10.0.0.5", msg.Ip);
        Assert.Equal(9999, msg.Port);
    }

    [Fact]
    public void Deserialize_Bye_RestoresFields()
    {
        var json = "{\"type\":\"bye\",\"name\":\"PC-C\",\"ip\":\"172.16.0.1\",\"port\":8080}";

        var msg = JsonSerializer.Deserialize<DiscoveryMessage>(json)!;

        Assert.Equal("bye", msg.Type);
        Assert.Equal("PC-C", msg.Name);
        Assert.Equal("172.16.0.1", msg.Ip);
        Assert.Equal(8080, msg.Port);
    }
}
