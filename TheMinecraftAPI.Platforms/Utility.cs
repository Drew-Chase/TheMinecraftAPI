namespace TheMinecraftAPI.Platforms;

public class Utility
{
    public static async Task<string> ConvertUrlImageToBase64(string url)
    {
        using HttpClient client = new();
        await using Stream stream = await client.GetStreamAsync(url);
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return $"data:image/png;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
    }
}