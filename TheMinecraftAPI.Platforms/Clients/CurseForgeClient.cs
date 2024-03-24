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
    private const string ApiKey = "$2a$10$qD2UJdpHaeDaQyGGaGS0QeoDnKq2EC7sX6YSjOxYHtDZSQRg04BCG";


    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, string gameVersion, int limit, int offset)
    {
        limit = Math.Clamp(limit, 0, 50);
        int classId = projectType switch
        {
            "mod" => 6,
            "modpack" => 4471,
            "customization" => 4546,
            "bukkit" => 5,
            "resourcepack" => 12,
            "shader" => 6552,
            "world" => 17,
            // These don't work in the API???
            "addon" => 4559,
            "datapack" => 6945,
            _ => 0
        };
        if (classId == 0) return PlatformSearchResults.Empty;
        loader = string.IsNullOrWhiteSpace(loader) ? "" : $"&modLoaderType={loader}";
        gameVersion = string.IsNullOrWhiteSpace(gameVersion) ? "" : $"&gameVersion={gameVersion}";
        JObject? json = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"{BaseUrl}/mods/search?gameId={GameId}&classId={classId}&searchFilter={query}&pageSize={limit}&index={offset}{loader}{gameVersion}&sortOrder=desc"),
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

        projects.Reverse();
        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            Limit = json["pageSize"]?.ToObject<int>() ?? limit,
            Offset = json["index"]?.ToObject<int>() ?? offset,
            Query = query,
            TotalResults = json["pagination"]?["totalCount"]?.ToObject<int>() ?? projects.Count
        };
    }

    public async Task<PlatformSearchResults> AdvancedSearchProjects(string query, int limit, int offset, AdvancedSearchOptions options)
    {
        if (options.Platforms.Length != 0 && !options.Platforms.Any(i => i.ToLower().Equals("curseforge"))) return PlatformSearchResults.Empty;
        List<Task<PlatformSearchResults>> results = new();
        options.ProjectTypes = options.ProjectTypes.Length == 0 ? new[] { "mod", "modpack", "resourcepack" } : options.ProjectTypes;

        foreach (var type in options.ProjectTypes)
        {
            string loaderStr = "", versionStr = "";
            if (options.Loaders.Length > 0)
            {
                foreach (var loader in options.Loaders)
                    loaderStr = loader;
            }

            if (options.MinecraftVersions.Length > 0)
            {
                foreach (var version in options.MinecraftVersions)
                    versionStr = version;
            }

            results.Add(SearchProjects(query, type, loaderStr, versionStr, limit, offset));
        }

        await Task.WhenAll(results.ToArray());

        List<PlatformModel> projects = new();
        int totalResults = 0;
        foreach (var item in results.Select(task => task.Result).Where(item => !item.IsEmpty))
        {
            projects.AddRange(item.Results);
            totalResults += item.TotalResults;
        }


        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            TotalResults = totalResults,
            Limit = limit,
            Offset = offset,
            Query = query
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
            return json is null ? PlatformModel.Empty : await FromJson(json["data"]?.ToObject<JObject>() ?? new JObject(), "");
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
            if (int.TryParse(file["modLoader"]?.ToString(), out int loaderType))
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

        JObject? bodyJson = await _client.GetAsJson(new HttpRequestMessage
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

        foreach (var author in json["authors"] ?? new JArray())
        {
            if (author is not JObject user) continue;
            string profileUrl = $"https://www.curseforge.com/members/{user["name"]?.ToString() ?? string.Empty}/projects";

            authors.Add(new Author
            {
                Id = user["id"]?.ToString() ?? string.Empty,
                Name = user["name"]?.ToString() ?? string.Empty,
                Url = profileUrl,
            });
        }

        foreach (var screenshot in json["screenshots"] ?? new JArray())
        {
            if (screenshot is not JObject image) continue;
            gallery.Add(new GalleryImageModel
            {
                Name = image["title"]?.ToString() ?? string.Empty,
                Description = image["description"]?.ToString() ?? string.Empty,
                Url = image["url"]?.ToString() ?? string.Empty,
                Created = DateTime.MinValue
            });
        }


        return new PlatformModel
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
            Type = type,
            Authors = authors.ToArray(),
            Gallery = gallery.ToArray(),
            Sides = new SupportedSides()
            {
                Client = "unknown",
                Server = "unknown",
            },
            Platforms = new[]
            {
                new PlatformSource
                {
                    Id = json["id"]?.ToString() ?? string.Empty,
                    Name = "CurseForge"
                }
            },
            Links = new[]
            {
                new PlatformLink
                {
                    Name = "Website",
                    Url = json["links"]?["websiteUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink
                {
                    Name = "Wiki",
                    Url = json["links"]?["wikiUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink
                {
                    Name = "Issues",
                    Url = json["links"]?["issuesUrl"]?.ToString() ?? string.Empty
                },
                new PlatformLink
                {
                    Name = "Source",
                    Url = json["links"]?["sourceUrl"]?.ToString() ?? string.Empty
                }
            }.Where(i => !string.IsNullOrWhiteSpace(i.Url)).ToArray()
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