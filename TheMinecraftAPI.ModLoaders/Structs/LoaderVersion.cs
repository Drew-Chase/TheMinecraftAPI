using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.ModLoaders.Structs;

/// <summary>
/// Represents a loader version.
/// </summary>
public struct LoaderVersion
{
    /// <summary>
    /// Represents a version of a mod loader.
    /// </summary>
    [JsonProperty("version")]
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Represents a version of Minecraft supported by a mod loader.
    /// </summary>
    [JsonProperty("minecraft-version")]
    [JsonPropertyName("minecraft-version")]
    public string? MinecraftVersion { get; set; }

    /// <summary>
    /// Represents a loader version file.
    /// </summary>
    [JsonProperty("files")]
    [JsonPropertyName("files")]
    public LoaderVersionFile[]? Files { get; set; }
}

/// <summary>
/// Represents a file associated with a loader version.
/// </summary>
public struct LoaderVersionFile
{
    /// <summary>
    /// Represents a file in a loader version.
    /// </summary>
    [JsonProperty("file-name")]
    [JsonPropertyName("file-name")]
    public string FileName { get; set; }

    /// <summary>
    /// Represents the URL property of a LoaderVersionFile.
    /// </summary>
    /// <value>
    /// The URL of the loader version file.
    /// </value>
    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public Uri Url { get; set; }
}