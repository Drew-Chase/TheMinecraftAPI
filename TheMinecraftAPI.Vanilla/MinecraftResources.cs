using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using TheMinecraftAPI.Vanilla.Structs;

namespace TheMinecraftAPI.Vanilla;

public enum JREOperatingSystem
{
    All,
    Windows,
    MacOS,
    Linux,
}

public enum JREArchitecture
{
    All,
    x64,
    x86,
    ARM,
}

public static class MinecraftResources
{
    public static async Task<object[]> GetJreBinaries(string operatingSystem)
    {
        const string url = "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";
        using AdvancedNetworkClient httpClient = new();
        JObject? response = await httpClient.GetAsJson(url);
        if (response is null) throw new Exception("Failed to get JRE binaries");


        List<object> results = new();

        foreach (var (os, value) in response)
        {
            if (os == "gamecore") continue;

            // filter by operating system and architecture
            if (!string.IsNullOrWhiteSpace(operatingSystem) && os != operatingSystem) continue;
            try
            {
                var alphaManifestUrl = value?["java-runtime-alpha"]?[0]?["manifest"]?["url"]?.ToString();
                var alphaName = value?["java-runtime-alpha"]?[0]?["version"]?["name"]?.ToString();
                var alphaReleased = value?["java-runtime-alpha"]?[0]?["version"]?["released"]?.ToString();

                if (alphaManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(alphaManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = alphaName,
                        released = alphaReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var betaManifestUrl = value?["java-runtime-beta"]?[0]?["manifest"]?["url"]?.ToString();
                var betaName = value?["java-runtime-beta"]?[0]?["version"]?["name"]?.ToString();
                var betaReleased = value?["java-runtime-beta"]?[0]?["version"]?["released"]?.ToString();
                if (betaManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(betaManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = betaName,
                        released = betaReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var deltaManifestUrl = value?["java-runtime-delta"]?[0]?["manifest"]?["url"]?.ToString();
                var deltaName = value?["java-runtime-delta"]?[0]?["version"]?["name"]?.ToString();
                var deltaReleased = value?["java-runtime-delta"]?[0]?["version"]?["released"]?.ToString();
                if (deltaManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(deltaManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = deltaName,
                        released = deltaReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var gammaManifestUrl = value?["java-runtime-gamma"]?[0]?["manifest"]?["url"]?.ToString();
                var gammaName = value?["java-runtime-gamma"]?[0]?["version"]?["name"]?.ToString();
                var gammaReleased = value?["java-runtime-gamma"]?[0]?["version"]?["released"]?.ToString();
                if (gammaManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(gammaManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = gammaName,
                        released = gammaReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var gammaSnapshotManifestUrl = value?["java-runtime-gamma-snapshot"]?[0]?["manifest"]?["url"]?.ToString();
                var gammaSnapshotName = value?["java-runtime-gamma-snapshot"]?[0]?["version"]?["name"]?.ToString();
                var gammaSnapshotReleased = value?["java-runtime-gamma-snapshot"]?[0]?["version"]?["released"]?.ToString();
                if (gammaSnapshotManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(gammaSnapshotManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = gammaSnapshotName,
                        released = gammaSnapshotReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                var legacyManifestUrl = value?["jre-legacy"]?[0]?["manifest"]?["url"]?.ToString();
                var legacyName = value?["jre-legacy"]?[0]?["version"]?["name"]?.ToString();
                var legacyReleased = value?["jre-legacy"]?[0]?["version"]?["released"]?.ToString();


                if (legacyManifestUrl is not null)
                {
                    object files = await GetJreBinaryFilesFromManifest(legacyManifestUrl);
                    results.Add(new
                    {
                        platform = os,
                        version = legacyName,
                        released = legacyReleased,
                        files
                    });
                }
            }
            catch
            {
                // ignored
            }
        }

        return results.ToArray();
    }

    public static async Task<string[]> GetSupportedPlatforms()
    {
        const string url = "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json";
        using AdvancedNetworkClient httpClient = new();
        JObject? response = await httpClient.GetAsJson(url);
        if (response is null) throw new Exception("Failed to get JRE binaries");

        List<string> results = new();
        foreach (var (os, value) in response)
        {
            if (os == "gamecore") continue;
            results.Add(os);
        }

        return results.ToArray();
    }

    /// <summary>
    /// Retrieves the JRE binary files from a specified manifest URL.
    /// </summary>
    /// <param name="manifestUrl">The URL of the JRE manifest.</param>
    /// <returns>An array containing the JRE binary files.</returns>
    private static async Task<object[]> GetJreBinaryFilesFromManifest(string manifestUrl)
    {
        using AdvancedNetworkClient httpClient = new();
        JObject? response = await httpClient.GetAsJson(manifestUrl);
        if (response is null) throw new Exception("Failed to read JRE manifest");
        JObject? files = response["files"] as JObject;
        List<object> results = new();
        if (files is null) return results.ToArray();

        foreach (var (fileName, value) in files)
        {
            string type = value?["type"]?.ToString() ?? throw new Exception("Failed to read JRE manifest");
            if (type != "file") continue;

            try
            {
                string? rawUrl = value?["downloads"]?["raw"]?["url"]?.ToString();
                string? lzmaUrl = value?["downloads"]?["lzma"]?["url"]?.ToString();
                string? rawSha1 = value?["downloads"]?["raw"]?["sha1"]?.ToString();
                string? lzmaSha1 = value?["downloads"]?["lzma"]?["sha1"]?.ToString();
                long? rawSize = long.Parse(value?["downloads"]?["raw"]?["size"]?.ToString() ?? "0");
                long? lzmaSize = long.Parse(value?["downloads"]?["lzma"]?["size"]?.ToString() ?? "0");
                results.Add(new
                {
                    fileName,
                    raw = new
                    {
                        url = rawUrl,
                        sha1 = rawSha1,
                        size = rawSize
                    },
                    lzma = new
                    {
                        url = lzmaUrl,
                        sha1 = lzmaSha1,
                        size = lzmaSize
                    }
                });
            }
            catch
            {
                // ignored
            }
        }


        return results.ToArray();
    }


    /// <summary>
    /// Retrieves the available versions of Minecraft.
    /// </summary>
    /// <param name="filter">Optional filter for a specific version.</param>
    /// <param name="snapshots">Indicates if only snapshot versions should be included.</param>
    /// <returns>An object containing the list of available versions.</returns>
    public static async Task<MinecraftVersionHistory> GetVersions(Version? filter = null, bool snapshots = false)
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

        List<MinecraftVersion> releasesList = new();
        List<MinecraftVersion> snapshotsList = new();

        foreach (var jToken in versions)
        {
            var version = (JObject)jToken;
            string id = version["id"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
            string type = version["type"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
            DateTime time = DateTime.Parse(version["time"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions"));
            DateTime releaseTime = DateTime.Parse(version["releaseTime"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions"));
            bool isSnapshot = type == "snapshot";
            if (isSnapshot)
            {
                if (snapshots && filter is null)
                    snapshotsList.Add(new MinecraftVersion
                    {
                        Id = id,
                        Type = type,
                        Time = time,
                        ReleaseTime = releaseTime,
                        Latest = latestSnapshot == id
                    });
            }
            else
            {
                if (filter is not null)
                {
                    Version major = new(filter.Major, filter.Minor + 1);
                    if (Version.TryParse(id, out Version? v) && v >= filter && v < major)
                    {
                        releasesList.Add(new MinecraftVersion
                        {
                            Id = id,
                            Type = type,
                            Time = time,
                            ReleaseTime = releaseTime,
                            Latest = latestRelease == id
                        });
                    }
                }
                else
                {
                    releasesList.Add(new MinecraftVersion
                    {
                        Id = id,
                        Type = type,
                        Time = time,
                        ReleaseTime = releaseTime,
                        Latest = latestRelease == id
                    });
                }
            }
        }

        return new MinecraftVersionHistory
        {
            Releases = releasesList.ToArray(),
            Snapshots = snapshotsList.ToArray(),
        };
    }

    /// <summary>
    /// Retrieves the assets for a specified Minecraft version.
    /// </summary>
    /// <param name="version">The Minecraft version for which to retrieve the assets.</param>
    /// <returns>An array of objects containing information about the assets if successful; otherwise, throws an exception.</returns>
    public static async Task<object[]> GetAssets(string version)
    {
        JObject versionManifest = await GetVersionManifest(version);
        string assetUrl = versionManifest["assetIndex"]?["url"]?.ToString() ?? throw new Exception("Failed to get Minecraft asset manifest");
        using AdvancedNetworkClient httpClient = new();
        JObject? assetManifest = await httpClient.GetAsJson(assetUrl);
        if (assetManifest is null) throw new Exception("Failed to get Minecraft asset manifest");
        List<object> results = new();
        foreach (var (key, value) in assetManifest["objects"] as JObject ?? throw new Exception("Failed to get Minecraft asset manifest"))
        {
            if (value is null) continue;
            string hash = value["hash"]?.ToString() ?? throw new Exception("Failed to get Minecraft asset manifest");
            string size = value["size"]?.ToString() ?? throw new Exception("Failed to get Minecraft asset manifest");
            string url = $"https://resources.download.minecraft.net/{hash[..2]}/{hash}";
            results.Add(new
            {
                file_name = key,
                hash,
                size,
                url
            });
        }

        return results.ToArray();
    }

    /// <summary>
    /// Retrieves the client and server jars for a specified Minecraft version.
    /// </summary>
    /// <param name="version">The Minecraft version for which to retrieve the jars.</param>
    /// <returns>An anonymous object containing the URLs, SHA1 hashes, sizes, and mappings for the client and server jars.</returns>
    public static async Task<object> GetJars(string version)
    {
        JObject versionManifest = await GetVersionManifest(version);


        // Client
        string? clientUrl = versionManifest["downloads"]?["client"]?["url"]?.ToString();
        string? clientSha1 = versionManifest["downloads"]?["client"]?["sha1"]?.ToString();
        string? clientMappingsUrl = versionManifest["downloads"]?["client_mappings"]?["url"]?.ToString();
        long clientSize = long.Parse(versionManifest["downloads"]?["client"]?["size"]?.ToString() ?? "0");

        // Server
        string? serverUrl = versionManifest["downloads"]?["server"]?["url"]?.ToString();
        string? serverSha1 = versionManifest["downloads"]?["server"]?["sha1"]?.ToString();
        string? serverMappingsUrl = versionManifest["downloads"]?["server_mappings"]?["url"]?.ToString();
        long serverSize = long.Parse(versionManifest["downloads"]?["server"]?["size"]?.ToString() ?? "0");
        return new
        {
            client = new
            {
                url = clientUrl,
                sha1 = clientSha1,
                size = clientSize,
                mappings = clientMappingsUrl
            },
            server = new
            {
                url = serverUrl,
                sha1 = serverSha1,
                size = serverSize,
                mappings = serverMappingsUrl
            }
        };
    }

    /// <summary>
    /// Retrieves the version manifest for a specified Minecraft version.
    /// </summary>
    /// <param name="version">The Minecraft version for which to retrieve the manifest.</param>
    /// <returns>The version manifest as a JObject if successful; otherwise, throws an exception.</returns>
    private static async Task<JObject> GetVersionManifest(string version)
    {
        using AdvancedNetworkClient httpClient = new();
        JObject? response = await httpClient.GetAsJson("https://launchermeta.mojang.com/mc/game/version_manifest.json");
        if (response is null)
        {
            throw new Exception("Failed to get Minecraft versions");
        }

        JArray versions = response["versions"] as JArray ?? throw new Exception("Failed to get Minecraft versions");
        if (versions.FirstOrDefault(v => v["id"]?.ToString() == version) is not JObject selectedVersion) throw new Exception($"Version not found: {version}");
        string url = selectedVersion["url"]?.ToString() ?? throw new Exception("Failed to get Minecraft versions");
        JObject? versionResponse = await httpClient.GetAsJson(url);

        return versionResponse ?? throw new Exception("Failed to get Minecraft versions");
    }
}