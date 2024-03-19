using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TheMinecraftAPI.Vanilla;

namespace TheMinecraftAPI.Server.Controllers;

[Produces("application/json")]
[Route("/api/minecraft/")]
[ApiController]
public class VanillaController : ControllerBase
{
    [HttpGet("server")]
    public async Task<IActionResult> GetServerStatusAsync([FromQuery] string url, [FromQuery] int port = 25565)
    {
        MinecraftServers server = new MinecraftServers(url, port);
        var response = await server.GetServerStatusAsync();
        return new ContentResult
        {
            Content = response.ToString(),
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}