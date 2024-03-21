using System.Text.Json;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.structs;

public struct PlatformModel
{
    /// <summary>
    /// Gets or sets the ID of the platform.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    /// <summary>
    /// Represents the model for a platform in TheMinecraftAPI.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Represents the body of a platform.
    /// </summary>
    [JsonProperty("body")]
    public string Body { get; set; }

    /// <summary>
    /// Represents the number of downloads for a platform.
    /// </summary>
    [JsonProperty("downloads")]
    public long Downloads { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the platform.
    /// </summary>
    /// <value>
    /// The icon.
    /// </value>
    [JsonProperty("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// Represents the authors of a platform.
    /// </summary>
    [JsonProperty("authors")]
    public string[] Authors { get; set; }

    /// <summary>
    /// Represents a platform model.
    /// </summary>
    [JsonProperty("tags")]
    public string[] Tags { get; set; }

    /// <summary>
    /// Represents the categories of a platform.
    /// </summary>
    [JsonProperty("categories")]
    public string[] Categories { get; set; }

    /// <summary>
    /// Represents the links associated with a platform.
    /// </summary>
    [JsonProperty("links")]
    public PlatformLink[] Links { get; set; }

    /// <summary>
    /// Represents a platform in the Minecraft API.
    /// </summary>
    [JsonProperty("platforms")]
    public PlatformSource[] Platforms { get; set; }

    /// <summary>
    /// Represents a gallery of images associated with a platform model.
    /// </summary>
    [JsonProperty("gallery")]
    public GalleryImageModel[] Gallery { get; set; }

    /// <summary>
    /// Represents the date and time the platform was created.
    /// </summary>
    [JsonProperty("created")]
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the platform model was last updated.
    /// </summary>
    /// <value>
    /// The date and time of the last update.
    /// </value>
    [JsonProperty("updated")]
    public DateTime Updated { get; set; }
}