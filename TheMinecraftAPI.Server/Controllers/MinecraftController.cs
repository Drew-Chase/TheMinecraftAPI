using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheMinecraftAPI.Vanilla;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/minecraft/")]
[ApiController]
public class MinecraftController : ControllerBase
{
    /// <summary>
    /// Retrieves the status of a Minecraft server asynchronously.
    /// </summary>
    /// <param name="url">The URL of the Minecraft server.</param>
    /// <param name="port">The port number of the Minecraft server. Default value is 25565.</param>
    /// <returns>A task representing the asynchronous operation. The task result is an object containing the server status information.</returns>
    [HttpGet("server"), ResponseCache(Duration = 60)] // Cache for 1 minute
    public async Task<IActionResult> GetServerStatusAsync([FromQuery] string url, [FromQuery] int port = 25565)
    {
        try
        {
            MinecraftServers server = new MinecraftServers(url, port);
            var response = await server.GetServerStatusAsync();
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(response),
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                ip = url,
                port = port,
                message = e.Message,
            });
        }
    }

    /// <summary>
    /// Retrieves the available versions of Minecraft.
    /// </summary>
    /// <param name="majorVersion">Optional filter for a specific major version.</param>
    /// <param name="snapshots">Indicates if only snapshot versions should be included.</param>
    /// <returns>An object containing the list of available versions.</returns>
    [HttpGet("versions"), ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<IActionResult> GetVersionsAsync([FromQuery(Name = "major_version")] Version? majorVersion = null, [FromQuery] bool snapshots = false)
    {
        var response = await MinecraftResources.GetVersions(majorVersion, snapshots);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the Java Runtime Environment (JRE) binaries for the specified operating system asynchronously.
    /// </summary>
    /// <param name="operatingSystem">The operating system to retrieve the JRE binaries for.
    /// Leave empty or specify "platforms" to retrieve the list of supported platforms.</param>
    /// <returns>A task representing the asynchronous operation. The task result is an IActionResult
    /// containing the JRE binaries or a list of supported platforms.</returns>
    [HttpGet("java/{operatingSystem?}"), ResponseCache(Duration = 86400)] // Cache for 24 hours
    public async Task<IActionResult> GetJavaAsync([FromRoute] string operatingSystem = "")
    {
        if (!string.IsNullOrWhiteSpace(operatingSystem))
        {
            string[] supportedPlatforms = await MinecraftResources.GetSupportedPlatforms();

            if (operatingSystem == "platforms")
            {
                return Ok(supportedPlatforms);
            }

            if (!supportedPlatforms.Contains(operatingSystem))
            {
                return BadRequest(new
                {
                    message = $"Invalid operating system: '{operatingSystem}'",
                    supported_platforms = supportedPlatforms,
                });
            }
        }

        try
        {
            var response = await MinecraftResources.GetJreBinaries(operatingSystem);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                message = e.Message,
                stacktrace = e.StackTrace,
            });
        }
    }

    /// <summary>
    /// Retrieves the assets of a specific Minecraft version.
    /// </summary>
    /// <param name="version">The version of Minecraft.</param>
    /// <returns>A task representing the asynchronous operation. The task result is an array of objects representing the assets.</returns>
    [HttpGet("version/{version}/assets"), ResponseCache(Duration = 86400)] // Cache for 24 hours
    public async Task<IActionResult> GetAssets([FromRoute] string version)
    {
        try
        {
            var response = await MinecraftResources.GetAssets(version);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                message = e.Message,
                stacktrace = e.StackTrace,
            });
        }
    }

    /// <summary>
    /// Retrieves the available JAR files for a specific version of Minecraft.
    /// </summary>
    /// <param name="version">The version of Minecraft.</param>
    /// <returns>An object containing the list of available JAR files.</returns>
    [HttpGet("version/{version}/jars"), ResponseCache(Duration = 86400)] // Cache for 24 hours
    public async Task<IActionResult> GetJars([FromRoute] string version)
    {
        try
        {
            var response = await MinecraftResources.GetJars(version);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                message = e.Message,
                stacktrace = e.StackTrace,
            });
        }
    }
}