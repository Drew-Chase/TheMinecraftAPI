namespace TheMinecraftAPI.Server.Data;

/// <summary>
/// Provides static properties representing various directories used in the application.
/// </summary>
public static class Directories
{
    /// <summary>
    /// Represents the root directory used in the application.
    /// </summary>
    public static string Root { get; } = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")).FullName;

    /// <summary>
    /// Provides static properties representing various directories used in the application.
    /// </summary>
    public static string Logs { get; } = Directory.CreateDirectory(Path.Combine(Root, "logs")).FullName;
}