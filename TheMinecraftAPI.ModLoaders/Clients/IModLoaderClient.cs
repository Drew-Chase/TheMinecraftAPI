using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.ModLoaders.Clients;

/// <summary>
/// Represents a mod loader client.
/// </summary>
public interface IModLoaderClient : IDisposable
{
    /// <summary>
    /// Retrieves the available loader installers.
    /// </summary>
    /// <param name="gameVersion">Optional. The Minecraft game version. If specified, only installers compatible with this version will be returned.</param>
    /// <returns>An array of LoaderVersion objects representing the available loader installers.</returns>
    public Task<LoaderVersion[]> GetInstallers(string? gameVersion = null);

    /// <summary>
    /// Retrieves the installer(s) for the specified version and game version.
    /// </summary>
    /// <param name="versionId">The version ID of the installer to retrieve.</param>
    /// <param name="gameVersion">The optional game version for filtering the installers.</param>
    /// <returns>An array of LoaderVersion objects representing the installer(s).</returns>
    public Task<LoaderVersion[]> GetInstaller(string versionId, string? gameVersion = null);

    /// <summary>
    /// Retrieves the available versions of the mod loader.
    /// </summary>
    /// <param name="gameVersion">[Optional] The Minecraft game version. If specified, filters the versions by the specified game version.</param>
    /// <returns>An array of string representing the available versions of the mod loader.</returns>
    public Task<string[]> GetVersions(string? gameVersion = null);
}