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
                RequestUri = new Uri($"{BaseUrl}/v1/mods/{id}"),
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

    public async Task<string> GetProjectIcon(string id)
    {
        JObject? json = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"{BaseUrl}/v1/mods/{id}"),
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
        string profileUrl = $"{BaseUrl}/members/{username}/projects";
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

    public async Task<PlatformVersion[]> GetProjectVersions(string id, string[] gameVersions, string[] loaders, ReleaseType[] releaseTypes, int limit, int offset)
    {
        var data = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"https://api.curseforge.com/v1/mods/{id}/files"),
            Method = HttpMethod.Get,
            Headers = { { "x-api-key", ApiKey } }
        });
        if (data is null) return Array.Empty<PlatformVersion>();
        List<PlatformVersion> versions = new();
        foreach (var item in data["data"] ?? new JArray())
        {
            if (item is not JObject version) continue;
            PlatformVersion platformVersion = FromJson(version);
            if (platformVersion.IsEmpty) continue;

            bool isGameVersionMatched = !gameVersions.Any() || platformVersion.GameVersions.Intersect(gameVersions).Any();
            bool isLoaderMatched = !loaders.Any() || platformVersion.Loaders.Intersect(loaders).Any();
            bool isReleaseTypeMatched = !releaseTypes.Any() || releaseTypes.Contains(platformVersion.ReleaseType);

            if (isGameVersionMatched && isLoaderMatched && isReleaseTypeMatched)
            {
                versions.Add(platformVersion);
            }
        }

        return limit > 0 ? versions.Skip(offset).Take(limit).ToArray() : versions.ToArray();
    }

    public async Task<PlatformVersion> GetProjectVersion(string id, string versionId)
    {
        var json = await _client.GetAsJson(new HttpRequestMessage()
        {
            RequestUri = new Uri($"https://api.curseforge.com/v1/mods/{id}/files/{versionId}"),
            Method = HttpMethod.Get,
            Headers = { { "x-api-key", ApiKey } }
        });
        if (json?["data"] is not { } data) return PlatformVersion.Empty;
        if (data is JObject obj)
            return FromJson(obj);
        return PlatformVersion.Empty;
    }

    private static PlatformVersion FromJson(JObject json)
    {
        var versions = json["gameVersions"]?.ToObject<List<string>>() ?? new List<string>();
        var loaders = versions.Where(i => UniversalClient.SupportedLoaders.Any(c => c.Equals(i, StringComparison.OrdinalIgnoreCase))).ToArray();
        versions = versions.Except(loaders).ToList();

        return new PlatformVersion()
        {
            Id = json["id"]?.ToString() ?? string.Empty,
            Name = json["displayName"]?.ToString() ?? string.Empty,
            Changelog = "",
            ProjectId = json["modId"]?.ToString() ?? string.Empty,
            GameVersions = versions.ToArray(),
            Loaders = loaders,
            ReleaseType = json["releaseType"]?.ToObject<int>() switch
            {
                1 => ReleaseType.Release,
                2 => ReleaseType.Beta,
                3 => ReleaseType.Alpha,
                _ => ReleaseType.Unknown
            },
            UploadDate = json["fileDate"]?.ToObject<DateTime>() ?? DateTime.MinValue,
            Downloads = json["downloadCount"]?.ToObject<int>() ?? 0,
            Files = new[]
            {
                new PlatformVersionFile()
                {
                    FileName = json["fileName"]?.ToString() ?? string.Empty,
                    FileSize = json["fileLength"]?.ToObject<long>() ?? 0,
                    Url = json["downloadUrl"]?.ToObject<Uri>(),
                    Primary =true,
                    Hash = json["hashes"]?[0]?["value"]?.ToString() ?? string.Empty,
                }
            },
            Version = json["displayName"]?.ToString() ?? string.Empty,
            Dependencies = json["dependencies"]?.ToArray()?.Select(i=>
                new PlatformVersionDependencies()
                {
                    Id = i["modId"]?.ToString() ?? string.Empty,
                    VersionId = null,
                    Type = i["type"]?.ToObject<int>() switch
                    {
                        1 => PlatformVersionDependencyType.Embedded,
                        2 => PlatformVersionDependencyType.Optional,
                        3 => PlatformVersionDependencyType.Required,
                        _ => PlatformVersionDependencyType.Unknown
                    }
                }).ToArray() ?? Array.Empty<PlatformVersionDependencies>(),

        };
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