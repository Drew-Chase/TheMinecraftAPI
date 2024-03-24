using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TheMinecraftAPI.Platforms.Structs;

public struct AdvancedSearchOptions
{
    /// <summary>
    /// Represents the available project types for advanced project search.
    /// </summary>
    [JsonProperty("project-types")]
    [JsonPropertyName("project-types")]
    public string[] ProjectTypes { get; set; }

    /// <summary>
    /// Represents the advanced search options for platform projects.
    /// </summary>
    [JsonProperty("platforms")]
    [JsonPropertyName("platforms")]
    public string[] Platforms { get; set; }

    /// <summary>
    /// Represents a set of advanced search options for filtering platform projects.
    /// </summary>
    [JsonProperty("categories")]
    [JsonPropertyName("categories")]
    public string[] Categories { get; set; }

    /// Represents the Minecraft versions to filter the search results by.
    /// /
    [JsonProperty("minecraft-versions")]
    [JsonPropertyName("minecraft-versions")]
    public string[] MinecraftVersions { get; set; }

    /// <summary>
    /// Represents the advanced search options for a search query.
    /// </summary>
    [JsonProperty("loaders")]
    [JsonPropertyName("loaders")]
    public string[] Loaders { get; set; }

    /// <summary>
    /// Gets or sets the datetime value representing the earliest creation timestamp for the projects.
    /// </summary>
    /// <value>
    /// The datetime value representing the earliest creation timestamp for the projects.
    /// </value>
    /// <remarks>
    /// You can use this property to filter projects based on their creation date. Only projects created after this
    /// datetime will be included in the search results.
    /// </remarks>
    [JsonProperty("created-after")]
    [JsonPropertyName("created-after")]
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Gets or sets the date and time before which the projects were created.
    /// </summary>
    [JsonProperty("created-before")]
    [JsonPropertyName("created-before")]
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Gets or sets the maximum date and time for the project's last update.
    /// </summary>
    [JsonProperty("updated-before")]
    [JsonPropertyName("updated-before")]
    public DateTime? UpdatedBefore { get; set; }

    /// <summary>
    /// Represents the date and time after which the projects should have been last updated.
    /// </summary>
    [JsonProperty("updated-after")]
    [JsonPropertyName("updated-after")]
    public DateTime? UpdatedAfter { get; set; }

    /// <summary>
    /// Represents the client-side property used for advanced search options.
    /// </summary>
    [JsonProperty("client-side")]
    [JsonPropertyName("client-side")]
    public bool ClientSide { get; set; }

    /// <summary>
    /// Represents the server-side property of the <see cref="AdvancedSearchOptions"/> struct.
    /// </summary>
    /// <value>
    /// <c>true</c> if the project has server-side functionality; otherwise, <c>false</c>.
    /// </value>
    [JsonProperty("server-side")]
    [JsonPropertyName("server-side")]
    public bool ServerSide { get; set; }

    /// <summary>
    /// Represents the advanced search options for a ModrinthClient.
    /// </summary>
    [JsonProperty("deep-search")]
    [JsonPropertyName("deep-search")]
    public bool DeepSearch { get; set; }
}