using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.ModLoaders;

public interface IModLoaderClient
{
    public Task<LoaderVersion[]> GetVersions();
    public Task<LoaderVersion[]> GetVersion(string versionId);
}