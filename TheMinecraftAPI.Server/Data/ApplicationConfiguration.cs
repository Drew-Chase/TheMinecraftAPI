using Chase.CommonLib.FileSystem.Configuration;
using Newtonsoft.Json;
using Serilog.Events;

namespace TheMinecraftAPI.Server.Data;

/// <summary>
/// Represents the configuration settings for the application.
/// </summary>
public class ApplicationConfiguration : AppConfigBase<ApplicationConfiguration>
{
    /// <summary>
    /// Represents the port used by the application.
    /// </summary>
    /// <remarks>
    /// The port indicates the network port number on which the application listens for incoming requests.
    /// </remarks>
    [JsonProperty("port")] public int Port { get; set; } = 8080;

    /// <summary>
    /// Represents the encryption salt used for encrypting data.
    /// </summary>
    [JsonProperty("encryption-key")] public string EncryptionSalt { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Represents the log level for logging events.
    /// </summary>
    [JsonProperty("log-level")] public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Represents the startup time of the application.
    /// </summary>
    [JsonIgnore] public DateTime StartupTime { get; } = DateTime.Now;
}