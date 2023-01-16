using ChatGPT.Net.Enums;

namespace ChatGPT.Net.DTO;

public class ChatGptClientConfig
{
    public string SessionToken { get; set; }
    public AccountType AccountType { get; set; } = AccountType.Free;
}