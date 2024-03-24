using System.Text;

namespace TheMinecraftAPI.Platforms;

public class FacetBuilder
{
    private readonly StringBuilder _builder;

    internal bool IsEmpty { get; private set; }

    public FacetBuilder()
    {
        _builder = new StringBuilder();
        _builder.Append("facets=[");
        IsEmpty = true;
    }

    /// <summary>
    /// Adds a facet for modloaders. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="loaders">A list of minecraft mod loaders</param>
    /// <returns></returns>
    public FacetBuilder AddModloaders(params string[] loaders) => AddCategories(Array.ConvertAll(loaders, i => i.ToString().ToLower()));

    /// <summary>
    /// Adds a facet for categories. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="categories">A list of modrinth categories</param>
    /// <returns></returns>
    public FacetBuilder AddCategories(params string[] categories)
    {
        IsEmpty = false;
        StringBuilder categoryBuilder = new();
        _builder.Append('[');
        foreach (string category in categories)
        {
            categoryBuilder.Append("\"categories:");
            categoryBuilder.Append(category);
            categoryBuilder.Append("\",");
        }

        _builder.Append(categoryBuilder.ToString().Trim(','));
        _builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for versions. If you add them all in one it will be considered an 'OR', or you
    /// can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="versions">A list of minecraft versions</param>
    /// <returns></returns>
    public FacetBuilder AddVersions(params string[] versions)
    {
        IsEmpty = false;
        StringBuilder versionBuilder = new();
        _builder.Append('[');
        foreach (string version in versions)
        {
            versionBuilder.Append("\"versions:");
            versionBuilder.Append(version);
            versionBuilder.Append("\",");
        }

        _builder.Append(versionBuilder.ToString().Trim(','));
        _builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for licenses. If you add them all in one it will be considered an 'OR', or you
    /// can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="licenses">A list of license types</param>
    /// <returns></returns>
    public FacetBuilder AddLicenses(params string[] licenses)
    {
        IsEmpty = false;
        StringBuilder licenceBuilder = new();
        _builder.Append('[');
        foreach (string license in licenses)
        {
            licenceBuilder.Append("\"license:");
            licenceBuilder.Append(license);
            licenceBuilder.Append("\",");
        }

        _builder.Append(licenceBuilder.ToString().Trim(','));
        _builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a facet for project types. If you add them all in one it will be considered an 'OR', or
    /// you can add them with individual function calls for an 'AND' request <br/><a
    /// href="https://docs.modrinth.com/docs/tutorials/api_search/#or">Modrinths Search API Documentation</a>
    /// </summary>
    /// <param name="types">A list of project types. Ex: mod, modpack, resourcepack, etc</param>
    /// <returns></returns>
    public FacetBuilder AddProjectTypes(params string[] types)
    {
        IsEmpty = false;
        StringBuilder typeBuilder = new();
        _builder.Append('[');
        foreach (string type in types)
        {
            typeBuilder.Append("\"project_type:");
            typeBuilder.Append(type);
            typeBuilder.Append("\",");
        }

        _builder.Append(typeBuilder.ToString().Trim(','));
        _builder.Append("],");

        return this;
    }

    /// <summary>
    /// Adds a custom facet filter to the FacetBuilder.
    /// </summary>
    /// <param name="key">The key for the custom facet.</param>
    /// <param name="op">The operator for the custom facet.</param>
    /// <param name="value">The value for the custom facet.</param>
    /// <returns>The updated FacetBuilder object.</returns>
    public FacetBuilder AddCustom(string key, string op, string value)
    {
        IsEmpty = false;
        _builder.Append('[');
        _builder.Append($"\"{key} {op} {value}\"");
        _builder.Append("],");

        return this;
    }

    /// <summary>
    /// Returns the finalized string output.
    /// </summary>
    /// <returns></returns>
    public string Build()
    {
        return _builder.ToString().Trim(',') + "]";
    }

    /// <summary>
    /// Returns the finalized string output. <br/> See: <seealso cref="Build">Build()</seealso>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Build();
    }
}