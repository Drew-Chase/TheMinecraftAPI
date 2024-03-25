﻿using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.Platforms.Clients;
using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Server.Controllers;

[Route("platform")]
[ApiController]
public class PlatformController : ControllerBase
{
    /// <summary>
    /// Advanced search for projects based on options.
    /// </summary>
    /// <param name="options">The advanced search options.</param>
    /// <param name="query">The search query. Default: "".</param>
    /// <param name="limit">The maximum number of results to return. Default: 10. Range: 0-100.</param>
    /// <param name="offset">The number of results to skip before starting to return. Default: 0.</param>
    /// <returns>A Task representing the asynchronous operation. The result contains the search results as JSON.</returns>
    [HttpPost("search"), ResponseCache(Duration = 3600)] // Cache the search for 1 hour
    [ProducesResponseType(typeof(PlatformSearchResults), 200)]
    [ProducesResponseType(typeof(string), 400)]
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

    /// <summary>
    /// Search for projects based on search query, project type, loader, game version, limit, offset.
    /// </summary>
    /// <param name="type">The project type.</param>
    /// <param name="loader">The project loader.</param>
    /// <param name="query">The search query.</param>
    /// <param name="gameVersion">The game version.</param>
    /// <param name="limit">The maximum number of results to return. Default: 10. Range: 0-100.</param>
    /// <param name="offset">The number of results to skip before starting to return. Default: 0.</param>
    /// <returns>A Task representing the asynchronous operation. The result contains the search results as JSON.</returns>
    [HttpGet("{type}/search"), ResponseCache(Duration = 3600)] // Cache the search for 1 hour
    [ProducesResponseType(typeof(PlatformSearchResults), 200)]
    [ProducesResponseType(typeof(string), 400)]
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


    /// <summary>
    /// Retrieves a project based on its ID and type.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="type">The type of the project.</param>
    /// <returns>The requested project.</returns>
    [HttpGet("{type}/project/{id}"), ResponseCache(Duration = 3600)] // Cache the project for 1 hour
    [ProducesResponseType(typeof(PlatformModel), 200)]
    [ProducesResponseType(typeof(string), 400)]
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

    /// <summary>
    /// Retrieves the icon of a project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="type">The type of project.</param>
    /// <returns>The project icon as a PNG image.</returns>
    [HttpGet("{type}/project/{id}/icon"), ResponseCache(Duration = 3600)] // Cache the icon for 1 hour
    [ProducesResponseType(typeof(string), 400)]
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

    /// <summary>
    /// Retrieves the raw project icon for a specific project.
    /// </summary>
    /// <param name="id">The id of the project.</param>
    /// <param name="type">The type of the project.</param>
    /// <returns>An IActionResult containing the project icon image as raw data.</returns>
    [HttpGet("{type}/project/{id}/icon/raw"), ResponseCache(Duration = 3600)] // Cache the icon for 1 hour
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
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