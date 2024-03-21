using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheMinecraftAPI.Vanilla;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/minecraft/")]
[ApiController]
public class MinecraftController : ControllerBase
{
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

    [HttpGet("versions"), ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<IActionResult> GetVersionsAsync([FromQuery(Name = "major_version")] Version? majorVersion = null, [FromQuery] bool snapshots = false)
    {
        var response = await MinecraftResources.GetVersions(majorVersion, snapshots);
        return Ok(response);
    }

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