namespace TheMinecraftAPI.Authentication.Entities;

public struct UserEntity()
{
    public Guid ApiToken { get; set; }
    public string AccessToken { get; set; }
}