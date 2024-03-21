using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public interface IPlatformClient
{
    /// <summary>
    /// Searches projects based on the specified query, project type, loader, limit, and offset.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="projectType">The project type to filter by.</param>
    /// <param name="loader">The loader to filter by.</param>
    /// <param name="limit">The maximum number of projects to retrieve.</param>
    /// <param name="offset">The number of projects to skip from the beginning of the results.</param>
    /// <returns>An array of <see cref="PlatformModel"/> representing the search results.</returns>
    public Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, int limit, int offset);

    /// <summary>
    /// Retrieves a platform project by its ID.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve.</param>
    /// <param name="type">The project type, (mod, modpack, resourcepack, etc.)</param>
    /// <returns>The PlatformModel object representing the project.</returns>
    public Task<PlatformModel> GetProject(string id, string type);

    /// <summary>
    /// Retrieves the icon URL for a platform project by its ID and type.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="type">The type of the project (mod, modpack, resourcepack, etc.).</param>
    /// <returns>The URL of the project icon.</returns>
    public Task<string> GetProjectIcon(string id, string type);
    public Task<string> GetAuthorProfileImage(string username);
}