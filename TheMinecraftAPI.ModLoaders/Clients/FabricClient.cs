using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.ModLoaders.Clients;

public class FabricClient : IModLoaderClient
{
    private AdvancedNetworkClient _client = new();
    private const string BaseUrl = "https://meta.fabricmc.net/v2/";

    public async Task<LoaderVersion[]> GetInstallers(string? gameVersion = null)
    {
        if (await _client.GetAsJson($"{BaseUrl}versions") is not { } json) return Array.Empty<LoaderVersion>();
        List<LoaderVersion> versions = new();
        if (json["installer"] is not JArray installers) return versions.ToArray();
        foreach (var installer in installers)
        {
            string url = installer["url"]?.ToString() ?? "";
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? downloadUri)) continue;
            string filename = url.Split('/').Last();
            var version = new LoaderVersion
            {
                Version = installer["version"]?.ToString() ?? "",
                MinecraftVersion = installer["gameVersion"]?.ToString(),
                Files = new LoaderVersionFile[]
                {
                    new()
                    {
                        FileName = filename,
                        Url = downloadUri,
                    }
                }
            };
            versions.Add(version);
        }

        return versions.ToArray();
    }

    public async Task<LoaderVersion[]> GetInstaller(string versionId, string? gameVersion = null)
    {
        return (await GetInstallers()).Where(version => version.Version == versionId).ToArray();
    }

    public async Task<string[]> GetVersions(string? gameVersion = null)
    {
        if (await _client.GetAsJson($"{BaseUrl}versions") is not { } json) return Array.Empty<string>();
        if (json["loader"] is JArray loaders)
            return loaders.Select(loader => loader["version"]?.ToString() ?? "").ToArray();
        return Array.Empty<string>();
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}