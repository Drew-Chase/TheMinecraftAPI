using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

/// <summary>
/// Represents a platform version.
/// </summary>
public struct PlatformVersion
{
    /// <summary>
    /// Represents the identifier of a platform version.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Represents a project ID.
    /// </summary>
    [JsonPropertyName("project-id")]
    [JsonProperty("project-id")]
    public required string ProjectId { get; set; }

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonPropertyName("version")]
    [JsonProperty("version")]
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the upload date of the platform version.
    /// </summary>
    /// <value>
    /// The upload date of the platform version.
    /// </value>
    /// <remarks>
    /// Represents the date and time when the platform version was last updated or uploaded.
    /// </remarks>
    [JsonPropertyName("update-date")]
    [JsonProperty("update-date")]
    public required DateTime UploadDate { get; set; }

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Represents a platform version, including information about the version, files, game versions, loaders, and dependencies.
    /// </summary>
    [JsonPropertyName("files")]
    [JsonProperty("files")]
    public required PlatformVersionFile[] Files { get; set; }

    /// <summary>
    /// Represents a version of a game platform.
    /// </summary>
    [JsonPropertyName("game-versions")]
    [JsonProperty("game-versions")]
    public required string[] GameVersions { get; set; }

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonPropertyName("loaders")]
    [JsonProperty("loaders")]
    public required string[] Loaders { get; set; }

    /// <summary>
    /// Represents the type of a release.
    /// </summary>
    [JsonPropertyName("release-type")]
    [JsonProperty("release-type")]
    public required ReleaseType ReleaseType { get; set; }

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonPropertyName("downloads")]
    [JsonProperty("downloads")]
    public required int Downloads { get; set; }

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonPropertyName("changelog")]
    [JsonProperty("changelog")]
    public required string Changelog { get; set; }

    /// <summary>
    /// Represents a version of a platform.
    /// </summary>
    [JsonPropertyName("dependencies")]
    [JsonProperty("dependencies")]
    public required PlatformVersionDependencies[] Dependencies { get; set; }

    /// <summary>
    /// Represents an empty PlatformVersion.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore] [System.Text.Json.Serialization.JsonIgnore]
    public static readonly PlatformVersion Empty = new()
    {
        Id = string.Empty,
        ProjectId = string.Empty,
        Name = string.Empty,
        Version = "",
        Changelog = "",
        UploadDate = DateTime.MinValue,
        Files = Array.Empty<PlatformVersionFile>(),
        GameVersions = Array.Empty<string>(),
        Loaders = Array.Empty<string>(),
        ReleaseType = ReleaseType.Unknown,
        Downloads = 0,
        Dependencies = Array.Empty<PlatformVersionDependencies>()
    };

    /// <summary>
    /// Determines whether the platform version is empty.
    /// </summary>
    /// <returns>
    /// True if the platform version is empty; otherwise, false.
    /// </returns>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty => Equals(Empty);
}