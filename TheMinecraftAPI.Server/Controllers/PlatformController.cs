using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Platforms.Clients;
using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Server.Controllers;

[Route("platform")]
[ApiController]
public class PlatformController : ControllerBase
{
    [HttpPost("search"), ResponseCache(Duration = 3600)] // Cache the search for 1 hour
    public async Task<IActionResult> AdvancedSearchProjects([FromBody] AdvancedSearchOptions options, [FromQuery] string query = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            limit = Math.Clamp(limit, 0, 100);
            offset = Math.Max(offset, 0);
            using UniversalClient client = new();
            var projects = await client.AdvancedSearchProjects(query, limit, offset, options);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{type}/search"), ResponseCache(Duration = 3600)] // Cache the search for 1 hour
    public async Task<IActionResult> SearchProjects([FromRoute(Name = "type")] string type, [FromQuery] string loader = "", [FromQuery] string query = "", [FromQuery] string gameVersion = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
    {
        try
        {
            limit = Math.Clamp(limit, 0, 100);
            offset = Math.Max(offset, 0);
            using UniversalClient client = new();
            var projects = await client.SearchProjects(query, type, loader, gameVersion, limit, offset);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("{type}/project/{id}"), ResponseCache(Duration = 3600)] // Cache the project for 1 hour
    public async Task<IActionResult> GetProject([FromRoute] string id, [FromRoute(Name = "type")] string type)
    {
        try
        {
            using UniversalClient client = new();
            var project = await client.GetProject(id, type);
            return Ok(project);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{type}/project/{id}/icon"), ResponseCache(Duration = 3600)] // Cache the icon for 1 hour
    public async Task<IActionResult> GetProjectIcon([FromRoute] string id, [FromRoute(Name = "type")] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type))
                return BadRequest("Invalid id or type.");

            using UniversalClient client = new();
            var icon = await client.GetProjectIcon(id);
            // Convert the base64 string to a byte array and return it as a png image
            return File(Convert.FromBase64String(icon[22..]), "image/png");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("{type}/project/{id}/icon/raw"), ResponseCache(Duration = 3600)] // Cache the icon for 1 hour
    public async Task<IActionResult> GetProjectIconRaw([FromRoute] string id, [FromRoute(Name = "type")] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type))
                return BadRequest("Invalid id or type.");

            using UniversalClient client = new();
            var icon = await client.GetProjectIcon(id);
            return Ok(icon);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}