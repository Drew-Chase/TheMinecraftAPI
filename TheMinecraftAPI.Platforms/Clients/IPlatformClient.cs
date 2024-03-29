﻿using TheMinecraftAPI.Platforms.Structs;

namespace TheMinecraftAPI.Platforms.Clients;

public interface IPlatformClient
{
    /// <summary>
    /// Searches projects based on the specified query, project type, loader, limit, and offset.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="projectType">The project type to filter by.</param>
    /// <param name="loader">The loader to filter by.</param>
    /// <param name="gameVersion">The minecraft version.</param>
    /// <param name="limit">The maximum number of projects to retrieve.</param>
    /// <param name="offset">The number of projects to skip from the beginning of the results.</param>
    /// <returns>An array of <see cref="PlatformModel"/> representing the search results.</returns>
    public Task<PlatformSearchResults> SearchProjects(string query, string projectType, string loader, string gameVersion, int limit, int offset);

    /// <summary>
    /// Performs an advanced search for projects based on the specified search criteria.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="limit">The maximum number of projects to retrieve.</param>
    /// <param name="offset">The number of projects to skip before starting to retrieve.</param>
    /// <param name="options">The advanced search options.</param>
    /// <returns>The search results containing the matching projects.</returns>
    public Task<PlatformSearchResults> AdvancedSearchProjects(string query, int limit, int offset, AdvancedSearchOptions options);

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
    /// <returns>The URL of the project icon.</returns>
    public Task<string> GetProjectIcon(string id);

    /// <summary>
    /// Retrieves the URL of the author's profile image.
    /// </summary>
    /// <param name="username">The username of the author.</param>
    /// <returns>The URL of the author's profile image as a string.</returns>
    public Task<string> GetAuthorProfileImage(string username);

    /// <summary>
    /// Retrieves a list of project versions for a given project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="gameVersions">An array of game versions.</param>
    /// <param name="loaders">An array of loaders.</param>
    /// <param name="releaseTypes">An array of release types.</param>
    /// <param name="limit">The maximum number of versions to return.</param>
    /// <param name="offset">The number of versions to skip before returning the result.</param>
    /// <returns>An array of PlatformVersion objects representing the project versions.</returns>
    public Task<PlatformVersion[]> GetProjectVersions(string id, string[] gameVersions, string[] loaders, ReleaseType[] releaseTypes, int limit, int offset);

    /// <summary>
    /// Retrieves the details of a specific version of a project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="versionId">The ID of the version.</param>
    /// <returns>A <see cref="PlatformVersion"/> object representing the details of the specified version.</returns>
    public Task<PlatformVersion> GetProjectVersion(string id, string versionId);

}