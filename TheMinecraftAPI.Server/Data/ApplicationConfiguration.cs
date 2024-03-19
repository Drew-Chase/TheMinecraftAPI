using Chase.CommonLib.FileSystem.Configuration;
using Newtonsoft.Json;
using Serilog.Events;

namespace TheMinecraftAPI.Server.Data;

public class ApplicationConfiguration : AppConfigBase<ApplicationConfiguration>
{
    [JsonProperty("port")] public int Port { get; set; } = 8080;

    [JsonProperty("encryption-key")] public string EncryptionSalt { get; set; } = Guid.NewGuid().ToString("N");

    [JsonProperty("log-level")] public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    [JsonIgnore] public DateTime StartupTime { get; } = DateTime.Now;
}