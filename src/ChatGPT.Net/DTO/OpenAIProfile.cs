using Newtonsoft.Json;

namespace ChatGPT.Net.DTO;

public class OpenAiProfile
{
    [JsonProperty("https://api.openai.com/profile")]
    public OpenAiProfileDetails Details { get; set; }

    [JsonProperty("https://api.openai.com/auth")]
    public OpenAiAuth Auth { get; set; }

    [JsonProperty("iss")]
    public string Issuer { get; set; }

    [JsonProperty("sub")]
    public string Subject { get; set; }

    [JsonProperty("aud")]
    public string[] Audience { get; set; }

    [JsonProperty("iat")]
    public long IssuedAt { get; set; }

    [JsonProperty("exp")]
    public long Expires { get; set; }

    [JsonProperty("azp")]
    public string AuthorizedParty { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }
}

public class OpenAiProfileDetails
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("email_verified")]
    public bool EmailVerified { get; set; }

    [JsonProperty("geoip_country")]
    public string GeoipCountry { get; set; }
}

public class OpenAiAuth
{
    [JsonProperty("user_id")]
    public string UserId { get; set; }
}
