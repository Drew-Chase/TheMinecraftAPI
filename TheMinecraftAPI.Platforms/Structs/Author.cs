using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public struct Author
{
    /// <summary>
    /// Gets or sets the Id of the Author.
    /// </summary>
    /// <remarks>
    /// The Id represents the unique identifier of the author in the system.
    /// </remarks>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the author.
    /// </summary>
    /// <remarks>
    /// This property represents the name of the author.
    /// </remarks>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Represents the URL of an author.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}