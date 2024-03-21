using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.structs;

public struct PlatformLink
{
    /// <summary>
    /// Gets or sets the name of the platform link.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Represents a URL property for a platform link.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}