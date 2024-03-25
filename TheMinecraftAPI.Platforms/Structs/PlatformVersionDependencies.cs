using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

/// <summary>
/// Represents the dependencies of a platform version.
/// </summary>
public struct PlatformVersionDependencies
{
    /// <summary>
    /// Represents the version of a platform.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Represents the version ID of a platform version dependency.
    /// </summary>
    [JsonPropertyName("version-id")]
    [JsonProperty("version-id")]
    public required string? VersionId { get; set; }

    /// <summary>
    /// Represents the dependencies of a platform version.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public required PlatformVersionDependencyType Type { get; set; }
}