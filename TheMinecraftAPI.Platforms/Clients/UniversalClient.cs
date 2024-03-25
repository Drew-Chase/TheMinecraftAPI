using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public class UniversalClient : IDisposable, IPlatformClient
{

   public static readonly string[] SupportedLoaders =
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

    private readonly IPlatformClient[] _clients =
    {
        new ModrinthClient(),
        new CurseForgeClient(),
    };

    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, string gameVersion, int limit, int offset)
    {
        List<PlatformModel> projects = new();
        int totalResults = 0;
        var tasks = new Task<PlatformSearchResults>[_clients.Length];

        for (int i = 0; i < _clients.Length; i++)
        {
            var client = _clients[i];
            tasks[i] = client.SearchProjects(query, projectType, loader, gameVersion, limit, offset);
        }

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            PlatformSearchResults item = task.Result;
            if (item.IsEmpty) continue;
            projects.AddRange(item.Results);
            totalResults += item.TotalResults;
        }

        return new PlatformSearchResults()
        {
            Results = SortByNameFuzzy(query, projects.OrderByDescending(i => i.Downloads)).Take(limit).ToArray(),
            TotalResults = totalResults,
            Limit = limit,
            Offset = limit,
            Query = query
        };
    }

    public async Task<PlatformSearchResults> AdvancedSearchProjects(string query, int limit, int offset, AdvancedSearchOptions options)
    {
        List<PlatformModel> projects = new();
        int totalResults = 0;
        var tasks = new Task<PlatformSearchResults>[_clients.Length];

        for (int i = 0; i < _clients.Length; i++)
        {
            var client = _clients[i];
            tasks[i] = client.AdvancedSearchProjects(query, limit, offset, options);
        }

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            PlatformSearchResults item = task.Result;
            if (item.IsEmpty) continue;
            projects.AddRange(item.Results);
            totalResults += item.TotalResults;
        }


        return new PlatformSearchResults()
        {
            Results = SortByNameFuzzy(query, projects.OrderByDescending(i => i.Downloads)).Take(limit).ToArray(),
            TotalResults = totalResults,
            Limit = limit,
            Offset = limit,
            Query = query
        };
    }


    public async Task<PlatformModel> GetProject(string id, string type)
    {
        foreach (var client in _clients)
        {
            var project = await client.GetProject(id, type);
            if (!project.IsEmpty) return project;
        }

        return PlatformModel.Empty;
    }

    public async Task<string> GetProjectIcon(string id)
    {
        foreach (var client in _clients)
        {
            var icon = await client.GetProjectIcon(id);
            if (!string.IsNullOrWhiteSpace(icon)) return icon;
        }

        return string.Empty;
    }

    public async Task<string> GetAuthorProfileImage(string username)
    {
        foreach (var client in _clients)
        {
            var image = await client.GetAuthorProfileImage(username);
            if (!string.IsNullOrWhiteSpace(image)) return image;
        }

        return string.Empty;
    }

    public async Task<PlatformVersion[]> GetProjectVersions(string id, string[] gameVersions, string[] loaders, ReleaseType[] releaseTypes, int limit, int offset)
    {
        foreach (var client in _clients)
        {
            try
            {
                var result = await client.GetProjectVersions(id, gameVersions, loaders, releaseTypes, limit, offset);
                if (result.Length == 0) continue;
                return result;
            }
            catch
            {
                // ignored
            }
        }

        return Array.Empty<PlatformVersion>();
    }

    public async Task<PlatformVersion> GetProjectVersion(string id, string versionId)
    {

        foreach (var client in _clients)
        {
            try
            {
                var result = await client.GetProjectVersion(id, versionId);
                if (result.IsEmpty) continue;
                return result;
            }
            catch
            {
                // ignored
            }
        }
        return PlatformVersion.Empty;
    }

    private static IEnumerable<PlatformModel> SortByNameFuzzy(string query, IEnumerable<PlatformModel> projects)
    {
        // calculate the Levenshtein difference between the query and the project name
        return projects.OrderBy(i => CalculateLevenshteinDifference(query, i.Name));
    }

    public static int CalculateLevenshteinDifference(string a, string b) => CalculateLevenshteinDifference(a, b, a.Length, b.Length);

    private static int CalculateLevenshteinDifference(string a, string b, int m, int n)
    {
        while (true)
        {
            if (n == 0 || m == 0) return Math.Max(n, m);
            if (a[m - 1] != b[n - 1])
                return 1 + Math.Min(
                    Math.Min(CalculateLevenshteinDifference(a, b, m, n - 1),
                        CalculateLevenshteinDifference(a, b, m - 1, n)),
                    CalculateLevenshteinDifference(a, b, m - 1, n - 1));
            m--;
            n--;
        }
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var client in _clients)
        {
            if (client is IDisposable disposable) disposable.Dispose();
        }
    }
}