namespace TheMinecraftAPI.ModLoaders.Structs;

public enum LoaderFileType
{
    Server,
    Client,
    Installer
}

public struct LoaderVersion
{
    public string Version { get; set; }
    public string MinecraftVersion { get; set; }
    public LoaderVersionFile[] Files { get; set; }
}

public struct LoaderVersionFile
{
    public string FileName { get; set; }
    public Uri Url { get; set; }
    public LoaderFileType Type { get; set; }
}