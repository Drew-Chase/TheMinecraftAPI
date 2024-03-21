using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public struct PlatformSource
{
    /// <summary>
    /// Gets or sets the Id of the platform source.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Represents the Name property of the PlatformSource struct.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }
}