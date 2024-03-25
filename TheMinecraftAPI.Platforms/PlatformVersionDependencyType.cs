using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public enum PlatformVersionDependencyType
{
    /// <summary>
    /// Represents a dependency that is required by the platform version.
    /// </summary>
    [JsonPropertyName("required")] [JsonProperty("required")]
    Required,

    /// <summary>
    /// Represents a dependency that is optional for the platform version.
    /// </summary>
    [JsonPropertyName("optional")] [JsonProperty("optional")]
    Optional,

    /// <summary>
    /// Represents a dependency type of an embedded platform version.
    /// </summary>
    [JsonPropertyName("embedded")] [JsonProperty("embedded")]
    Embedded,

    /// <summary>
    /// Represents an unknown dependency type for a platform version.
    /// </summary>
    [JsonPropertyName("unknown")] [JsonProperty("unknown")]
    Unknown
}