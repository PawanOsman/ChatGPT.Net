using Newtonsoft.Json;

namespace ChatGPT.Net.DTO;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("picture")]
    public string Picture { get; set; }

    [JsonProperty("groups")]
    public string[] Groups { get; set; }

    [JsonProperty("features")]
    public string[] Features { get; set; }
}

public class Profile
{
    [JsonProperty("user")]
    public User User { get; set; }

    [JsonProperty("expires")]
    public DateTime Expires { get; set; }

    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }
}