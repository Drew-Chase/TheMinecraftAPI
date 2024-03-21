using System.Text.RegularExpressions;
using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using Serilog;
using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public partial class CurseForgeClient : IDisposable, IPlatformClient
{
    private readonly AdvancedNetworkClient _client = new();
    private const string BaseUrl = "https://api.curseforge.com/v1";
    private const int GameId = 432;
    private const int ModClassId = 6;
    private const int ModPackClassId = 4471;
    private const int ResourcePackClassId = 4472;
    private const string ApiKey = "$2a$10$qD2UJdpHaeDaQyGGaGS0QeoDnKq2EC7sX6YSjOxYHtDZSQRg04BCG";


    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, int limit, int offset)
    {
        int classId = projectType switch
        {
            "mod" => ModClassId,
            "modpack" => ModPackClassId,
            "resourcepack" => ResourcePackClassId,
            _ => 0
        };
        if (classId == 0) return PlatformSearchResults.Empty;
        JObject? json = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"{BaseUrl}/mods/search?gameId={GameId}&classId={classId}&searchFilter={query}&pageSize={limit}&index={offset}&modLoaderType={loader}"),
            Method = HttpMethod.Get,
            Headers = { { "x-api-key", ApiKey } }
        });
        if (json?["data"] is not JArray hits) return PlatformSearchResults.Empty;

        List<PlatformModel> projects = new();
        foreach (var item in hits)
        {
            if (item is not JObject project) continue;
            projects.Add(await FromJson(project, projectType));
        }

        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            Limit = json["pageSize"]?.ToObject<int>() ?? limit,
            Offset = json["index"]?.ToObject<int>() ?? offset,
            Query = query,
            TotalResults = json["totalCount"]?.ToObject<int>() ?? projects.Count
        };
    }


    public async Task<PlatformModel> GetProject(string id, string type)
    {
        try
        {
            JObject? json = await _client.GetAsJson(new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://api.curseforge.com/v1/mods/{id}"),
                Method = HttpMethod.Get,
                Headers = { { "x-api-key", ApiKey } }
            });
            return json is null ? PlatformModel.Empty : await FromJson(json, "");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve project from CurseForge: {Id}", id);
            return PlatformModel.Empty;
        }
    }

    public async Task<string> GetProjectIcon(string id, string type)
    {
        JObject? json = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"https://api.curseforge.com/v1/mods/{id}"),
            Method = HttpMethod.Get,
            Headers = { { "x-api-key", ApiKey } }
        });
        if (json is null) return "";

        if (json["logo"]?["url"]?.ToString() is { } url)
        {
            return Utility.ConvertUrlImageToBase64(url).Result;
        }

        return string.Empty;
    }

    public async Task<string> GetAuthorProfileImage(string username)
    {
        string profileUrl = $"https://www.curseforge.com/members/{username}/projects";
        try
        {
            string html = await _client.GetStringAsync(profileUrl);
            var match = ExtractProfileImageFromProfileHtml().Match(html);
            if (match.Success)
            {
                string avatarUrl = match.Value;
                return await Utility.ConvertUrlImageToBase64(avatarUrl);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve profile image for {Name}", username);
        }

        return string.Empty;
    }

    private async Task<PlatformModel> FromJson(JObject json, string type)
    {
        HashSet<string> gameVersions = new();
        HashSet<string> loaders = new();
        HashSet<string> categories = new();
        List<Author> authors = new();
        List<GalleryImageModel> gallery = new();

        foreach (var version in json["latestFilesIndexes"] ?? new JArray())
        {
            if (version is not JObject file || file["gameVersion"] is null) continue;
            if (int.TryParse(file["loaderType"]?.ToString(), out int loaderType))
            {
                loaders.Add(loaderType switch
                {
                    1 => "Forge",
                    4 => "Fabric",
                    5 => "Quilt",
                    6 => "NeoForge",
                    _ => "Unknown"
                });
            }

            gameVersions.Add(file["gameVersion"]?.ToString() ?? "");
        }

        JObject? bodyJson = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"{BaseUrl}/mods/{json["id"]}/description"),
            Method = HttpMethod.Get,
            Headers =
            {
                { "x-api-key", ApiKey }
            }
        });
        string body = "";
        if (bodyJson?["data"] is not null)
        {
            body = bodyJson["data"]?.ToString() ?? "";
        }

        foreach (var cat in json["categories"] ?? new JArray())
        {
            if (cat is not JObject category) continue;
            categories.Add(category["name"]?.ToString() ?? "");
        }

        // string avatarUrl = "https://static-cdn.jtvnw.net/user-default-pictures-uv/294c98b5-e34d-42cd-a8f0-140b72fba9b0-profile_image-150x150.png";

        foreach (var author in json["authors"] ?? new JArray())
        {
            if (author is not JObject user) continue;
            string profileUrl = $"https://www.curseforge.com/members/{user["name"]?.ToString() ?? string.Empty}/projects";

            authors.Add(new Author()
            {
                Id = user["id"]?.ToString() ?? string.Empty,
                Name = user["name"]?.ToString() ?? string.Empty,
                Url = profileUrl,
            });
        }

        foreach (var screenshot in json["screenshots"] ?? new JArray())
        {
            if (screenshot is not JObject image) continue;
            gallery.Add(new GalleryImageModel()
            {
                Name = image["title"]?.ToString() ?? string.Empty,
                Description = image["description"]?.ToString() ?? string.Empty,
                Url = image["url"]?.ToString() ?? string.Empty,
                Created = DateTime.MinValue
            });
        }

        return new PlatformModel()
        {
            Id = json["id"]?.ToString() ?? string.Empty,
            Name = json["name"]?.ToString() ?? string.Empty,
            Slug = json["slug"]?.ToString() ?? string.Empty,
            Description = json["summary"]?.ToString() ?? string.Empty,
            Downloads = json["downloadCount"]?.ToObject<int>() ?? 0,
            Created = json["dateCreated"]?.ToObject<DateTime>() ?? DateTime.MinValue,
            Updated = json["dateModified"]?.ToObject<DateTime>() ?? DateTime.MinValue,
            GameVersions = gameVersions.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray(),
            Versions = Array.Empty<string>(),
            Body = body,
            Loaders = loaders.ToArray(),
            Categories = categories.ToArray(),
            Tags = Array.Empty<string>(),
            Type = type,
            Authors = authors.ToArray(),
            Gallery = gallery.ToArray(),

            Platforms = new[]
            {
                new PlatformSource()
                {
                    Id = json["id"]?.ToString() ?? string.Empty,
                    Name = "CurseForge"
                }
            },
            Links = new[]
            {
                new PlatformLink()
                {
                    Name = "Website",
                    Url = json["websiteUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink()
                {
                    Name = "Wiki",
                    Url = json["wikiUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink()
                {
                    Name = "Issues",
                    Url = json["issuesUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink()
                {
                    Name = "Source",
                    Url = json["sourceUrl"]?.ToString() ?? string.Empty
                }
            }
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }

    [GeneratedRegex("(?<=\"avatarUrl\":\")https://static-cdn.jtvnw.net/jtv_user_pictures/((?!\").)*")]
    private static partial Regex ExtractProfileImageFromProfileHtml();
}