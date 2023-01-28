using ChatGPT.Net.Enums;

namespace ChatGPT.Net.DTO;

public class ChatGptClientConfig
{
    public string Name { get; set; } = "default";
    public string SessionToken { get; set; }
    public string ConfigDir { get; set; }
    public AccountType AccountType { get; set; } = AccountType.Free;
}