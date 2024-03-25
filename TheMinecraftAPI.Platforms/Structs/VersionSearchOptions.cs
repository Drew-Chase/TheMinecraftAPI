namespace TheMinecraftAPI.Platforms.Structs;

public struct VersionSearchOptions
{
    public string[] GameVersions {get;set;}
    public string[] Loaders {get;set;}
    public ReleaseType[] ReleaseTypes {get;set;}
}