using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using Serilog;
using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public class ModrinthClient : IDisposable, IPlatformClient
{
    private readonly AdvancedNetworkClient _client = new();
    private const string BaseUrl = "https://api.modrinth.com/v2";

    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, int limit, int offset)
    {
        string category = string.IsNullOrWhiteSpace(loader) ? "" : $",[\"loaders:{loader}\"]";
        JObject? json = await _client.GetAsJson($"{BaseUrl}/search?query={query}&limit={limit}&index=relevance&offset={offset}&facets=[[\"project_type:{projectType}\"]{category}]");
        if (json?["hits"] is not JArray hits) return PlatformSearchResults.Empty;
        List<PlatformModel> projects = new();
        foreach (var item in hits)
        {
            if (item is not JObject project) continue;
            string id = project["project_id"]?.ToString() ?? string.Empty;
            PlatformModel model = await GetProject(id, projectType);
            if (model.IsEmpty) continue;
            projects.Add(model);
        }


        return new PlatformSearchResults()
        {
            Results = projects.ToArray(),
            Limit = json["limit"]?.ToObject<int>() ?? limit,
            Offset = json["offset"]?.ToObject<int>() ?? offset,
            Query = query,
            TotalResults = json["total"]?.ToObject<int>() ?? projects.Count
        };
    }


    public async Task<PlatformModel> GetProject(string id, string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) return PlatformModel.Empty;
            JObject? json = await _client.GetAsJson($"{BaseUrl}/project/{id}");
            if (json is null) return PlatformModel.Empty;

            List<GalleryImageModel> gallery = new();
            var categories = json["categories"]?.ToObject<List<string>>() ?? new List<string>();
            categories.AddRange(json["additional_categories"]?.ToObject<List<string>>() ?? new List<string>());

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


            return new PlatformModel()
            {
                Id = json["id"]?.ToString() ?? id,
                Slug = json["slug"]?.ToString() ?? string.Empty,
                Name = json["title"]?.ToString() ?? string.Empty,
                Description = json["description"]?.ToString() ?? string.Empty,
                Body = json["body"]?.ToString() ?? string.Empty,
                Downloads = json["downloads"]?.ToObject<int>() ?? 0,
                Authors = authors.ToArray(),
                Tags = json["tags"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                Categories = categories.ToArray(),
                GameVersions = json["game_versions"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                Versions = json["loaders"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                Links = sources.ToArray(),
                Gallery = gallery.ToArray(),
                Type = json["project_type"]?.ToString() ?? type,
                Created = DateTime.Parse(json["published"]?.ToString() ?? "1970-01-01T00:00:00Z"),
                Updated = DateTime.Parse(json["updated"]?.ToString() ?? "1970-01-01T00:00:00Z"),
                Platforms = new[]
                {
                    new PlatformSource()
                    {
                        Id = json["id"]?.ToString() ?? id,
                        Name = "Modrinth",
                    }
                },
                Loaders = json["loaders"]?.ToObject<string[]>() ?? Array.Empty<string>()
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to retrieve project from Modrinth: {Id}", id);
            return PlatformModel.Empty;
        }
    }

    public async Task<string> GetProjectIcon(string id, string type)
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


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}