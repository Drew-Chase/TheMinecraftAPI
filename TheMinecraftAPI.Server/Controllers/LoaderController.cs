using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.ModLoaders.Clients;
using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.Server.Controllers;

/// <summary>
/// Represents a controller for interacting with mod loader functionality.
/// </summary>
[ApiController]
[Route("loader")]
public class LoaderController : ControllerBase
{
    /// <summary>
    /// Represents the available types of mod loaders.
    /// </summary>
    private static readonly string[] LoaderTypes = new[]
    {
        "fabric",
        "forge"
    };

    /// <summary>
    /// Retrieves the available loader types.
    /// </summary>
    /// <returns>An array of strings representing the available loader types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string[]), 200)]
    public IActionResult GetLoaderTypes()
    {
        return Ok(LoaderTypes);
    }

    /// <summary>
    /// Get the available versions of a mod loader.
    /// </summary>
    /// <param name="type">The type of mod loader ("fabric" or "forge").</param>
    /// <param name="gameVersion">Optional. The specific game version. If not provided, all versions will be returned.</param>
    /// <returns>An array of LoaderVersion objects representing the available versions of the mod loader.</returns>
    [HttpGet("{type}/loaders")]
    [ProducesResponseType(typeof(LoaderVersion[]), 200)]
    public async Task<IActionResult> GetLoaderVersions(string type, [FromQuery] string? gameVersion = null)
    {
        IModLoaderClient? client = type switch
        {
            "fabric" => new FabricClient(),
            "forge" => new ForgeClient(),
            _ => null
        };
        if (client is null) return NotFound(LoaderTypes);
        var versions = await client.GetVersions(gameVersion);
        client.Dispose();
        return Ok(versions);
    }

    /// <summary>
    /// Retrieves the available loader installers for a given loader type.
    /// </summary>
    /// <param name="type">The type of loader. Values can be "fabric" or "forge".</param>
    /// <param name="gameVersion">Optional. The game version for which to retrieve installers.</param>
    /// <returns>An IActionResult containing the available loader installers as an array of LoaderVersion objects.</returns>
    [HttpGet("{type}/installers")]
    [ProducesResponseType(typeof(LoaderVersion[]), 200)]
    public async Task<IActionResult> GetLoaderInstallers(string type, [FromQuery] string? gameVersion = null)
    {
        IModLoaderClient? client = type switch
        {
            "fabric" => new FabricClient(),
            "forge" => new ForgeClient(),
            _ => null
        };
        if (client is null) return NotFound(LoaderTypes);
        var versions = await client.GetInstallers(gameVersion);
        client.Dispose();
        return Ok(versions);
    }


    /// GetLoaderVersion method retrieves the information about a specific version of a mod loader.
    /// Parameters:
    /// - type: The type of mod loader (fabric or forge).
    /// - version: The version of the mod loader to retrieve.
    /// - gameVersion (optional): The game version for which to retrieve the mod loader version. If not provided, all versions will be retrieved.
    /// Returns:
    /// - IActionResult: The result of the operation.
    [HttpGet("{type}/installer/{version}")]
    [ProducesResponseType(typeof(LoaderVersion[]), 200)]
    public async Task<IActionResult> GetLoaderVersion([FromRoute] string type, [FromRoute] string version, [FromQuery] string? gameVersion = null)
    {
        IModLoaderClient? client = type switch
        {
            "fabric" => new FabricClient(),
            "forge" => new ForgeClient(),
            _ => null
        };
        if (client is null) return NotFound(LoaderTypes);
        var versions = await client.GetInstaller(version, gameVersion);
        client.Dispose();
        return Ok(versions);
    }

    /// <summary>
    /// Retrieves the ForgeWrapper.jar file.
    /// </summary>
    /// <returns>The ForgeWrapper.jar file as a PhysicalFileResult.</returns>
    [HttpGet("forge/wrapper")]
    public IActionResult GetForgeWrapper()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "ForgeWrapper.jar");
        if (!System.IO.File.Exists(filePath)) return NotFound();
        return PhysicalFile(filePath, "application/java-archive", "forge-wrapper.jar");
    }
}