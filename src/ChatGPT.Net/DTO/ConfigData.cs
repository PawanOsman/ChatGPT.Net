using ChatGPT.Net.Enums;

namespace ChatGPT.Net.DTO;

public class ConfigData
{
    public string Name { get; set; }
    public string SessionToken { get; set; }
    public string AccessToken { get; set; }
    public string Signature { get; set; }
    public List<ChatGptConversation> Conversations { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public AccountType AccountType { get; set; }
}