using Chase.CommonLib.Networking;
using Newtonsoft.Json.Linq;
using TheMinecraftAPI.Authentication.Entities;

namespace TheMinecraftAPI.Authentication;

public class AuthenticationManager
{
    public static async Task<UserEntity> RegisterGithubOAuth(string code)
    {
        JObject? json;
        using (AdvancedNetworkClient client = new())
        {
            // Get the access token from Github
            json = await client.GetAsJson(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://github.com/login/oauth/access_token?client_id={Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")}&client_secret={Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET")}&code={code}"),
                Headers = { { "User-Agent", "TheMinecraftAPI" }, { "Accept", "application/json" } },
            });
        }

        // Check if the access token is present
        if (json?["access_token"] is not { } token) throw new Exception("Failed to get access token from Github");

        // Return the user entity
        return new UserEntity()
        {
            ApiToken = Guid.NewGuid(),
            AccessToken = token.ToString(),
        };
    }
}