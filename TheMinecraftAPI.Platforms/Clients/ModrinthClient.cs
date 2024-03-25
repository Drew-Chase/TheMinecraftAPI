using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using Serilog;
using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public class ModrinthClient : IDisposable, IPlatformClient
{
    private readonly AdvancedNetworkClient _client = new();
    private const string BaseUrl = "https://api.modrinth.com/v2";

    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, string gameVersion, int limit, int offset)
    {
        FacetBuilder facets = new FacetBuilder()
            .AddProjectTypes(projectType);
        if (!string.IsNullOrWhiteSpace(loader))
            facets = facets.AddModloaders(loader);
        if (!string.IsNullOrWhiteSpace(gameVersion))
            facets = facets.AddVersions(gameVersion);
        JObject? json = await _client.GetAsJson($"{BaseUrl}/search?query={query}&limit={limit}&index=relevance&offset={offset}&{facets}");
        if (json?["hits"] is not JArray hits) return PlatformSearchResults.Empty;
        List<PlatformModel> projects = new();
        foreach (var item in hits)
        {
            if (item is not JObject project) continue;
            string id = project["project_id"]?.ToString() ?? string.Empty;
            PlatformModel model = await FromJson(project, projectType);
            if (model.IsEmpty) continue;
            projects.Add(model);
        }

        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            Limit = json["limit"]?.ToObject<int>() ?? limit,
            Offset = json["offset"]?.ToObject<int>() ?? offset,
            Query = query,
            TotalResults = json["total_hits"]?.ToObject<int>() ?? projects.Count
        };
    }

    public async Task<PlatformSearchResults> AdvancedSearchProjects(string query, int limit, int offset, AdvancedSearchOptions options)
    {
        if (options.Platforms.Length != 0 && !options.Platforms.Any(i => i.ToLower().Equals("modrinth"))) return PlatformSearchResults.Empty;
        FacetBuilder facets = new FacetBuilder();
        if (options.MinecraftVersions.Length > 0)
            facets = facets.AddVersions(options.MinecraftVersions);
        if (options.Categories.Length > 0)
            facets = facets.AddCategories(options.Categories);
        if (options.Loaders.Length > 0)
            facets = facets.AddModloaders(options.Loaders);
        if (options.ProjectTypes.Length > 0)
            facets = facets.AddProjectTypes(options.ProjectTypes);

        if (options.CreatedBefore is { } before && options.CreatedBefore != DateTime.MinValue)
            facets = facets.AddCustom("created_timestamp", "<", before.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        if (options.CreatedAfter is { } after && options.CreatedAfter != DateTime.MinValue)
            facets = facets.AddCustom("created_timestamp", ">", after.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        if (options.UpdatedBefore is { } updatedBefore && updatedBefore != DateTime.MinValue)
            facets = facets.AddCustom("updated_timestamp", "<", updatedBefore.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        if (options.UpdatedAfter is { } updatedAfter && updatedAfter != DateTime.MinValue)
            facets = facets.AddCustom("updated_timestamp", ">", updatedAfter.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        if (options.ClientSide)
            facets = facets.AddCustom("client_side", "=", "required");
        if (options.ServerSide)
            facets = facets.AddCustom("server_side", "=", "required");
        string url = $"{BaseUrl}/search?query={query}&limit={limit}&index=relevance&offset={offset}&{facets}";
        JObject? json = await _client.GetAsJson(url);
        if (json?["hits"] is not JArray hits) return PlatformSearchResults.Empty;
        List<PlatformModel> projects = new();
        foreach (var item in hits)
        {
            if (item is not JObject project) continue;
            string id = project["project_id"]?.ToString() ?? string.Empty;
            foreach (string projectType in options.ProjectTypes)
            {
                PlatformModel model = options.DeepSearch ? await GetProject(id, projectType) : await FromJson(project, projectType);
                if (model.IsEmpty) continue;
                projects.Add(model);
            }
        }

        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            Limit = json["limit"]?.ToObject<int>() ?? limit,
            Offset = json["offset"]?.ToObject<int>() ?? offset,
            Query = query,
            TotalResults = json["total_hits"]?.ToObject<int>() ?? projects.Count
        };
    }


    public async Task<PlatformModel> GetProject(string id, string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) return PlatformModel.Empty;
            JObject? json = await _client.GetAsJson($"{BaseUrl}/project/{id}");
            return json is null ? PlatformModel.Empty : await FromJson(json, type, true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve project from Modrinth: {Id}", id);
            return PlatformModel.Empty;
        }
    }

    private async Task<PlatformModel> FromJson(JObject json, string type, bool parseAuthors = false)
    {
        string[] supportedLoaders =
        {
            "bukkit",
            "bungeecord",
            "canvas",
            "datapack",
            "fabric",
            "folia",
            "forge",
            "iris",
            "liteloader",
            "minecraft",
            "modloader",
            "neoforge",
            "optifine",
            "paper",
            "purpur",
            "quilt",
            "rift",
            "spigot",
            "sponge",
            "vanilla",
            "velocity",
            "waterfall"
        };
        string id = json["project_id"]?.ToString() ?? json["id"]?.ToString() ?? string.Empty;
        List<GalleryImageModel> gallery = new();
        var categories = json["categories"]?.ToObject<List<string>>() ?? new List<string>();
        categories.AddRange(json["additional_categories"]?.ToObject<List<string>>() ?? new List<string>());

        string[] loaders = json["loaders"]?.ToObject<string[]>() ?? Array.Empty<string>();
        if (loaders.Length == 0)
        {
            loaders = categories.Where(i => supportedLoaders.Any(c => c.Equals(i, StringComparison.OrdinalIgnoreCase))).ToArray();
            categories = categories.Except(loaders).ToList();
        }

        foreach (var item in json["gallery"] ?? new JArray())
        {
            if (item is not JObject galleryItem) continue;

            string url = galleryItem["url"]?.ToString() ?? string.Empty;
            string galleryTitle = galleryItem["title"]?.ToString() ?? string.Empty;
            string galleryDescription = galleryItem["description"]?.ToString() ?? string.Empty;
            DateTime created = galleryItem["created"]?.ToObject<DateTime>() ?? DateTime.MinValue;

            gallery.Add(new GalleryImageModel()
            {
                Url = url,
                Name = galleryTitle,
                Description = galleryDescription,
                Created = created
            });
        }


        List<PlatformLink> sources = new();
        foreach (var item in json["donation_urls"] ?? new JArray())
        {
            if (item is not JObject donation) continue;

            string donationPlatform = donation["platform"]?.ToString() ?? string.Empty;
            string donationUrl = donation["url"]?.ToString() ?? string.Empty;

            sources.Add(new PlatformLink()
            {
                Name = donationPlatform,
                Url = donationUrl
            });
        }

        sources.AddRange(new[]
        {
            new PlatformLink()
            {
                Name = "Issues",
                Url = json["issues_url"]?.ToString() ?? string.Empty
            },
            new PlatformLink()
            {
                Name = "Source",
                Url = json["source_url"]?.ToString() ?? string.Empty
            },
            new PlatformLink()
            {
                Name = "Wiki",
                Url = json["wiki_url"]?.ToString() ?? string.Empty
            },
            new PlatformLink()
            {
                Name = "Discord",
                Url = json["discord_url"]?.ToString() ?? string.Empty
            }
        });

        List<Author> authors = new();
        if (parseAuthors)
        {
            JArray? members = await _client.GetAsJsonArray($"{BaseUrl}/project/{id}/members");
            foreach (var member in members ?? new JArray())
            {
                if (member is not JObject memberObject || memberObject["user"] is not JObject user) continue;
                authors.Add(new Author()
                {
                    Id = user["id"]?.ToString() ?? string.Empty,
                    Name = user["name"]?.ToString() ?? string.Empty,
                    Url = $"https://modrinth.com/user/{user["username"]?.ToString() ?? string.Empty}",
                });
            }
        }


        return new PlatformModel()
        {
            Id = json["id"]?.ToString() ?? id,
            Slug = json["slug"]?.ToString() ?? string.Empty,
            Name = json["title"]?.ToString() ?? string.Empty,
            Description = json["description"]?.ToString() ?? string.Empty,
            Body = json["body"]?.ToString() ?? string.Empty,
            Downloads = json["downloads"]?.ToObject<int>() ?? 0,
            Authors = authors.ToArray(),
            Categories = categories.ToArray(),
            GameVersions = json["game_versions"]?.ToObject<string[]>() ?? json["versions"]?.ToObject<string[]>() ?? Array.Empty<string>(),
            Versions = json["game_versions"] is not null ? json["versions"]?.ToObject<string[]>() ?? Array.Empty<string>() : Array.Empty<string>(),
            Links = sources.Where(i => !string.IsNullOrWhiteSpace(i.Url)).ToArray(),
            Gallery = gallery.ToArray(),
            Type = json["project_type"]?.ToString() ?? type,
            Created = DateTime.Parse(json["date_created"]?.ToString() ?? json["published"]?.ToString() ?? "1970-01-01T00:00:00Z"),
            Updated = DateTime.Parse(json["date_modified"]?.ToString() ?? json["updated"]?.ToString() ?? "1970-01-01T00:00:00Z"),
            Sides = new SupportedSides()
            {
                Client = json["client_side"]?.ToObject<string>() ?? "unknown",
                Server = json["server_side"]?.ToObject<string>() ?? "unknown",
            },
            Platforms = new[]
            {
                new PlatformSource()
                {
                    Id = json["id"]?.ToString() ?? id,
                    Name = "Modrinth",
                }
            },
            Loaders = loaders
        };
    }

    public async Task<string> GetProjectIcon(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return string.Empty;
        JObject? json = await _client.GetAsJson($"{BaseUrl}/project/{id}");

        if (json?["icon_url"] != null)
        {
            return await Utility.ConvertUrlImageToBase64(json["icon_url"]?.ToString() ?? string.Empty);
        }

        return string.Empty;
    }

    public async Task<string> GetAuthorProfileImage(string username)
    {
        JArray? user = await _client.GetAsJsonArray($"{BaseUrl}/user/{username}");
        if (user?["avatar_url"]?.ToString() is { } avatarUrl)
        {
            return await Utility.ConvertUrlImageToBase64(avatarUrl);
        }

        return string.Empty;
    }

    public async Task<PlatformVersion[]> GetProjectVersions(string id, string[] gameVersions, string[] loaders, ReleaseType[] releaseTypes, int limit, int offset)
    {
        var json = await _client.GetAsJsonArray($"{BaseUrl}/project/{id}/version");
        if (json is null) return Array.Empty<PlatformVersion>();
        List<PlatformVersion> versions = new();
        foreach (var item in json)
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
        JObject? json = await _client.GetAsJson($"{BaseUrl}/project/{id}/version/{versionId}");
        return json is null ? PlatformVersion.Empty : FromJson(json);
    }

    private static PlatformVersion FromJson(JObject json)
    {
        return new PlatformVersion()
        {
            Id = json["id"]?.ToString() ?? string.Empty,
            ProjectId = json["project_id"]?.ToString() ?? string.Empty,
            Name = json["name"]?.ToString() ?? string.Empty,
            Version = json["version_number"]?.ToString() ?? string.Empty,
            UploadDate = json["date_published"]?.ToObject<DateTime>() ?? DateTime.MinValue,
            Changelog = json["changelog"]?.ToString() ?? string.Empty,
            Downloads = json["downloads"]?.ToObject<int>() ?? 0,
            Loaders = json["loaders"]?.ToObject<string[]>() ?? Array.Empty<string>(),
            GameVersions = json["game_versions"]?.ToObject<string[]>() ?? Array.Empty<string>(),
            Dependencies = json["dependencies"]?.Select(i => new PlatformVersionDependencies()
            {
                Id = i["project_id"]?.ToString() ?? string.Empty,
                VersionId = i["version_id"]?.ToString(),
                Type = i["dependency_type"]?.ToString() switch
                {
                    "required" => PlatformVersionDependencyType.Required,
                    "optional" => PlatformVersionDependencyType.Optional,
                    "embedded" => PlatformVersionDependencyType.Embedded,
                    _ => PlatformVersionDependencyType.Unknown
                }
            }).ToArray() ?? Array.Empty<PlatformVersionDependencies>(),
            ReleaseType = json["release_type"]?.ToString() switch
            {
                "release" => ReleaseType.Release,
                "beta" => ReleaseType.Beta,
                "alpha" => ReleaseType.Alpha,
                _ => ReleaseType.Unknown
            },
            Files = json["files"]?.Select(i => new PlatformVersionFile()
            {
                Url = i["url"]?.ToObject<Uri>(),
                FileName = i["filename"]?.ToString() ?? "",
                FileSize = i["size"]?.ToObject<long>() ?? 0,
                Primary = i["primary"]?.ToObject<bool>() ?? false,
                Hash = i["hashes"]?["sha512"]?.ToString(),
            }).ToArray() ?? Array.Empty<PlatformVersionFile>()
        };
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}