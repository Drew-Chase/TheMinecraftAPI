using Microsoft.AspNetCore.Mvc;
using TheMinecraftAPI.ModLoaders.Clients;
using TheMinecraftAPI.ModLoaders.Structs;

namespace TheMinecraftAPI.Server.Controllers;

[ApiController]
[Route("loader")]
public class LoaderController : ControllerBase
{
    private static readonly string[] LoaderTypes = new[]
    {
        "fabric",
        "forge"
    };

    [HttpGet]
    [ProducesResponseType(typeof(string[]), 200)]
    public IActionResult GetLoaderTypes()
    {
        return Ok(LoaderTypes);
    }

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
    [HttpGet("forge/wrapper")]
    public IActionResult GetForgeWrapper()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "forge-wrapper.jar");
        if (!System.IO.File.Exists(filePath)) return NotFound();
        return PhysicalFile(filePath, "application/java-archive", "forge-wrapper.jar");
    }
}