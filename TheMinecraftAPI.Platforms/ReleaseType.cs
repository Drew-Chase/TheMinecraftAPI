using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

/// <summary>
/// Represents the type of a release.
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// Enum member representing a release type.
    /// </summary>
    [JsonPropertyName("release")] [JsonProperty("release")]
    Release,

    /// <summary>
    /// Represents the beta release type of a platform version.
    /// </summary>
    [JsonPropertyName("beta")] [JsonProperty("beta")]
    Beta,

    /// <summary>
    /// Represents the Alpha release type of a platform version.
    /// </summary>
    [JsonPropertyName("alpha")] [JsonProperty("alpha")]
    Alpha,

    /// <summary>
    /// The Unknown release type.
    /// </summary>
    [JsonPropertyName("unknown")] [JsonProperty("unknown")]
    Unknown
}