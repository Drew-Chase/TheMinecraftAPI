using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;

namespace TheMinecraftAPI.Vanilla;

public class MinecraftVersions
{
    public static async Task<object> GetVersions(Version? filter = null, bool snapshots = false)
    {
        using AdvancedNetworkClient httpClient = new();
        JObject? response = await httpClient.GetAsJson("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        if (response is null)
        {
            throw new Exception("Failed to get Minecraft versions");
        }

        JArray versions = response["versions"] as JArray ?? throw new Exception("Failed to get Minecraft versions");
        JObject latest = response["latest"] as JObject ?? throw new Exception("Failed to get Minecraft versions");
        string latestRelease = latest["release"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
        string latestSnapshot = latest["snapshot"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");

        List<object> releasesList = [];
        List<object> snapshotsList = [];

        foreach (var jToken in versions)
        {
            var version = (JObject)jToken;
            string id = version["id"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
            string type = version["type"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
            string url = version["url"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
            DateTime time = DateTime.Parse(version["time"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions"));
            DateTime releaseTime = DateTime.Parse(version["releaseTime"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions"));
            bool isSnapshot = type == "snapshot";
            if (isSnapshot)
            {
                if (snapshots && filter is null)
                    snapshotsList.Add(new
                    {
                        id,
                        type,
                        time,
                        releaseTime,
                        latest = latestSnapshot == id
                    });
            }
            else
            {
                if (filter is not null)
                {
                    Version major = new(filter.Major, filter.Minor + 1);
                    if (Version.TryParse(id, out Version? v) && v >= filter && v < major)
                    {
                        releasesList.Add(new
                        {
                            id,
                            type,
                            time,
                            releaseTime,
                            latest = latestRelease == id
                        });
                    }
                }
                else
                {
                    releasesList.Add(new
                    {
                        id,
                        type,
                        time,
                        releaseTime,
                        latest = latestRelease == id
                    });
                }
            }
        }

        return new
        {
            snapshots = snapshotsList,
            releases = releasesList,
        };
    }
}