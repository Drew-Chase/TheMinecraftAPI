using Chase.CommonLib.Networking;
using HtmlAgilityPack;
using Newtonsoft.Json;
using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.ModLoaders.Clients;

public class ForgeClient : IModLoaderClient
{
    private readonly AdvancedNetworkClient _client = new();
    private static readonly string ForgeJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "forge.json");

    public async Task<LoaderVersion[]> GetInstallers(string? gameVersion = null)
    {
        if (!File.Exists(ForgeJsonPath)) return Array.Empty<LoaderVersion>();
        string json = await File.ReadAllTextAsync(ForgeJsonPath);
        var versions = JsonConvert.DeserializeObject<LoaderVersion[]>(json) ?? Array.Empty<LoaderVersion>();
        return gameVersion is null ? versions : versions.Where(version => version.MinecraftVersion == gameVersion).ToArray();
    }

    public async Task<LoaderVersion[]> GetInstaller(string versionId, string? gameVersion = null)
    {
        return (await GetInstallers(gameVersion)).Where(version => version.Version == versionId).ToArray();
    }

    public async Task<string[]> GetVersions(string? gameVersion = null)
    {
        if (!File.Exists(ForgeJsonPath)) return Array.Empty<string>();
        string json = await File.ReadAllTextAsync(ForgeJsonPath);
        var versions = JsonConvert.DeserializeObject<LoaderVersion[]>(json) ?? Array.Empty<LoaderVersion>();
        return gameVersion is null ? versions
            .Select(version => version.Version).Distinct().ToArray() :
            versions.Where(version => version.MinecraftVersion == gameVersion).Select(version => version.Version).ToArray();
    }

    /// <summary>
    /// Updates the cache from the web for the given Minecraft versions.
    /// </summary>
    /// <param name="minecraftVersions">The Minecraft versions to update the cache for.</param>
    public static async Task UpdateCacheFromWeb(IEnumerable<string> minecraftVersions)
    {
        List<LoaderVersion> versions = new();
        using (AdvancedNetworkClient client = new())
        {
            const string url = "https://files.minecraftforge.net/net/minecraftforge/forge/";
            foreach (string version in minecraftVersions)
            {
                try
                {
                    string html = await client.GetStringAsync($"{url}index_{version}.html");
                    // Parse the HTML using AgilityPack
                    HtmlDocument document = new();
                    document.LoadHtml(html);
                    if (document.DocumentNode == null) continue;
                    HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//td[@class='download-version']");
                    foreach (HtmlNode node in nodes)
                    {
                        string versionNumber = node.InnerText.Trim('/').Trim();
                        if (!int.TryParse(versionNumber[0].ToString(), out _)) continue;
                        if (Uri.TryCreate($"https://maven.minecraftforge.net/net/minecraftforge/forge/{version}-{versionNumber}/forge-{version}-{versionNumber}-installer.jar", UriKind.Absolute, out Uri? versionUrl))
                            versions.Add(new LoaderVersion()
                            {
                                Version = versionNumber,
                                MinecraftVersion = version,
                                Files = new[]
                                {
                                    new LoaderVersionFile()
                                    {
                                        Url = versionUrl,
                                        FileName = $"forge-{version}-{versionNumber}-installer.jar"
                                    }
                                }
                            });
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        // Save the versions to a file
        await File.WriteAllTextAsync(ForgeJsonPath, JsonConvert.SerializeObject(versions));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}