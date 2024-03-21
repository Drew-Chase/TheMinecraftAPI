using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Platforms.Clients;

namespace TheMinecraftAPI.Server.Controllers;

[Route("platform/{type}")]
[ApiController]
public class PlatformController : ControllerBase
{
    [HttpGet("search"), ResponseCache(Duration = 3600)] // Cache the search for 1 hour
    public async Task<IActionResult> SearchProjects([FromRoute(Name = "type")] string type, [FromQuery] string loader = "", [FromQuery] string query = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            using UniversalClient client = new();
            var projects = await client.SearchProjects(query, type, loader, limit, offset);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("project/{id}"), ResponseCache(Duration = 3600)] // Cache the project for 1 hour
    public async Task<IActionResult> GetProject([FromRoute] string id, [FromRoute(Name = "type")] string type, [FromQuery] string loader)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(loader))
                return BadRequest("Invalid id, type, or loader.");

            using UniversalClient client = new();
            var project = await client.GetProject(id, type);
            return Ok(project);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("project/{id}/icon"), ResponseCache(Duration = 3600)] // Cache the icon for 1 hour
    public async Task<IActionResult> GetProjectIcon([FromRoute] string id, [FromRoute(Name = "type")] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type))
                return BadRequest("Invalid id or type.");

            using UniversalClient client = new();
            var icon = await client.GetProjectIcon(id, type);
            return Ok(icon);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}