namespace TheMinecraftAPI.Server.Data;

using static Directories;

/// <summary>
/// The <c>Files</c> class provides access to file paths used in the application.
/// </summary>
public static class Files
{
    /// <summary>
    /// Represents the configuration of the application.
    /// </summary>
    public static string ApplicationConfiguration { get; } = Path.Combine(Root, "application.json");

    /// <summary>
    /// Gets the path of the latest log file.
    /// </summary>
    /// <value>The path of the latest log file.</value>
    public static string LatestLog { get; } = Path.Combine(Logs, "latest.log");

    /// <summary>
    /// Represents the path to the error log file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property provides the path to the error log file. The error log file is used for logging error messages and exceptions.
    /// </para>
    /// <para>
    /// The error log file is named "error.log" and is located in the "logs" directory under the root directory of the application.
    /// </para>
    /// </remarks>
    public static string ErrorLog { get; } = Path.Combine(Logs, "error.log");

    /// <summary>
    /// Gets the path of the debug log file.
    /// </summary>
    /// <value>
    /// The path of the debug log file.
    /// </value>
    public static string DebugLog { get; } = Path.Combine(Logs, "debug.log");
}