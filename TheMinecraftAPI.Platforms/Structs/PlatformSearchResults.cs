using System.Text.Json.Serialization;

namespace TheMinecraftAPI.Platforms.Structs;

public struct PlatformSearchResults
{
    public PlatformModel[] Results { get; set; }
    public int TotalResults { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public string Query { get; set; }

    public static PlatformSearchResults Empty => new()
    {
        Results = Array.Empty<PlatformModel>(),
        TotalResults = 0,
        Limit = 0,
        Offset = 0,
        Query = string.Empty
    };

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public bool IsEmpty => Results.Length == 0;
}