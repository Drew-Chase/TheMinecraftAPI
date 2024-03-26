using System.Reflection;

namespace TheMinecraftAPI.Server.Data;

/// <summary>
/// Provides access to application-specific data.
/// </summary>
public static class ApplicationData
{
    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    /// <value>
    /// The name of the application.
    /// </value>
    public static string ApplicationName { get; } = "TheMinecraftAPI";

    /// <summary>
    /// Represents the uptime of the application.
    /// </summary>
    public static TimeSpan UpTime => DateTime.Now - ApplicationConfiguration.Instance.StartupTime;

    /// <summary>
    /// Provides information about the main assembly of the application.
    /// </summary>
    public static Assembly MainAssembly { get; } = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Provides information about the assembly.
    /// </summary>
    public static AssemblyName? AssemblyName { get; } = MainAssembly.GetName();

    /// <summary>
    /// Represents the version of the application.
    /// </summary>
    public static Version? Version { get; } = AssemblyName.Version;

    /// <summary>
    /// Generates application data.
    /// </summary>
    /// <returns>An anonymous object containing the application data.</returns>
    public static object GenerateApplicationData()
    {
        return new
        {
            ApplicationName,
            Version,
            UpTime,
            ApplicationConfiguration.Instance.StartupTime,
            Environment = "RELEASE",
            Config = ApplicationConfiguration.Instance,
        };
    }
}