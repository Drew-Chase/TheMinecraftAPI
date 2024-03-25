using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

/// <summary>
/// Represents a file in a platform version.
/// </summary>
public struct PlatformVersionFile
{
    /// <summary>
    /// Represents a URL property of a <see cref="PlatformVersionFile"/>.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonProperty("url")]
    public required Uri? Url { get; set; }

    /// <summary>
    /// Represents the file name of a platform version.
    /// </summary>
    [JsonPropertyName("file-name")]
    [JsonProperty("file-name")]
    public required string FileName { get; set; }

    /// <summary>
    /// Represents the file size of a platform version file.
    /// </summary>
    [JsonPropertyName("file-size")]
    [JsonProperty("file-size")]
    public required long FileSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this file is the primary file.
    /// </summary>
    /// <value>
    /// <c>true</c> if this file is the primary file; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("primary")]
    [JsonProperty("primary")]
    public required bool Primary { get; set; }

    /// <summary>
    /// Represents a file hash.
    /// </summary>
    [JsonPropertyName("hash")]
    [JsonProperty("hash")]
    public required string? Hash { get; set; }
}