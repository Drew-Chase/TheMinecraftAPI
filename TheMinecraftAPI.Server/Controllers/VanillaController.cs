using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheMinecraftAPI.Vanilla;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/minecraft/")]
[ApiController]
public class VanillaController : ControllerBase
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
                error = e.Message
            });
        }
    }

    [HttpGet("versions"), ResponseCache(Duration = 3600)] // Cache for 1 hour
    public async Task<IActionResult> GetVersionsAsync([FromQuery(Name = "major_version")] Version? majorVersion = null, [FromQuery] bool snapshots = false)
    {
        var response = await MinecraftVersions.GetVersions(majorVersion, snapshots);
        return Ok(response);
    }
}