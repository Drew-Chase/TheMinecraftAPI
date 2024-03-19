namespace TheMinecraftAPI.Server.Data;

public static class Directories
{
    public static string Root { get; } = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")).FullName;
    public static string Logs { get; } = Directory.CreateDirectory(Path.Combine(Root, "logs")).FullName;
}