using System.Text.Json.Serialization;

namespace LanDrop.Models;

public class TransferMetadata
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}
