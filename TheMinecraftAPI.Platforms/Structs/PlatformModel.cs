using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public struct PlatformModel
{
    /// <summary>
    /// Gets or sets the ID of the platform.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Represents a property that holds the slug value of a platform model.
    /// </summary>
    [JsonProperty("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// Gets or sets the name of the platform.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

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
    /// Represents the authors of a platform.
    /// </summary>
    [JsonProperty("authors")]
    public Author[] Authors { get; set; }


    /// <summary>
    /// Represents the categories of a platform.
    /// </summary>
    [JsonProperty("categories")]
    public string[] Categories { get; set; }

    /// <summary>
    /// Represents a platform model.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

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

    /// <summary>
    /// Represents a platform version.
    /// </summary>
    [JsonProperty("versions")]
    public string[] Versions { get; set; }

    /// <summary>
    /// Represents the game versions associated with a platform model.
    /// </summary>
    [JsonProperty("game_versions")]
    public string[] GameVersions { get; set; }

    /// <summary>
    /// Represents a platform model.
    /// </summary>
    [JsonProperty("loaders")]
    public string[] Loaders { get; set; }

    /// <summary>
    /// Represents the supported sides of a platform.
    /// </summary>
    [JsonProperty("sides")]
    public SupportedSides Sides { get; set; }

    /// <summary>
    /// Represents an empty instance of the <see cref="PlatformModel"/> struct.
    /// </summary>
    public static readonly PlatformModel Empty = new()
    {
        Id = string.Empty,
        Name = string.Empty,
        Description = string.Empty,
        Body = string.Empty,
        Downloads = 0,
        Authors = Array.Empty<Author>(),
        Categories = Array.Empty<string>(),
        Links = Array.Empty<PlatformLink>(),
        Platforms = Array.Empty<PlatformSource>(),
        Gallery = Array.Empty<GalleryImageModel>(),
        Created = DateTime.MinValue,
        Updated = DateTime.MinValue,
        Versions = Array.Empty<string>(),
        GameVersions = Array.Empty<string>(),
        Loaders = Array.Empty<string>()
    };

    /// <summary>
    /// Represents an empty PlatformModel with default values for all properties.
    /// </summary>
    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty => Id == string.Empty;
}