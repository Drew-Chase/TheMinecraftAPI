namespace TheMinecraftAPI.Vanilla.Structs;

public struct MinecraftVersionHistory
{
    public MinecraftVersion[] Releases { get; set; }
    public MinecraftVersion[] Snapshots { get; set; }
}

public struct MinecraftVersion
{
    public string Id {get;set;}
    public string Type {get;set;}
    public DateTime Time {get;set;}
    public DateTime ReleaseTime {get;set;}
    public bool Latest { get; set; }
}