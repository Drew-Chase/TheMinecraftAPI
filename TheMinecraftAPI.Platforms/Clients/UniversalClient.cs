using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public class UniversalClient : IDisposable, IPlatformClient
{
    private readonly IPlatformClient[] _clients =
    {
        new ModrinthClient(),
        new CurseForgeClient()
    };

    public async Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, int limit, int offset)
    {
        List<PlatformModel> projects = new();
        int totalResults = 0;
        int totalLimit = 0;
        int totalOffset = 0;
        var tasks = new Task<PlatformSearchResults>[_clients.Length];

        for (int i = 0; i < _clients.Length; i++)
        {
            var client = _clients[i];
            tasks[i] = client.SearchProjects(query, projectType, loader, limit, offset);
        }

        await Task.WhenAll(tasks);

        foreach (var task in tasks)
        {
            PlatformSearchResults item = task.Result;
            if (item.IsEmpty) continue;
            projects.AddRange(item.Results);
            totalResults += item.TotalResults;
            totalLimit += item.Limit;
            totalOffset += item.Offset;
        }

        return new PlatformSearchResults()
        {
            Results = projects.OrderByDescending(i => i.Downloads).ToArray(),
            TotalResults = totalResults,
            Limit = totalLimit,
            Offset = totalOffset,
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

    public async Task<string> GetProjectIcon(string id, string type)
    {
        foreach (var client in _clients)
        {
            var icon = await client.GetProjectIcon(id, type);
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


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var client in _clients)
        {
            if (client is IDisposable disposable) disposable.Dispose();
        }
    }
}