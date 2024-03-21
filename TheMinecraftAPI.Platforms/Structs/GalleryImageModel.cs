using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public struct GalleryImageModel
{
    /// <summary>
    /// Represents the name of a gallery image.
    /// </summary>
    /// <remarks>
    /// This property is used in the <see cref="GalleryImageModel"/> struct to store the name of the image.
    /// </remarks>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Represents a gallery image model.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Represents the URL property of a GalleryImageModel.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Represents the date and time when the gallery image was created.
    /// </summary>
    /// <remarks>
    /// This property is of type DateTime and is used in the GalleryImageModel struct.
    /// It indicates the exact point in time when the gallery image was created.
    /// </remarks>
    [JsonProperty("created")]
    public DateTime Created { get; set; }
}