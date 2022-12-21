namespace ChatGPT.Net.DTO;

public class ChatGptClientConfig
{
    public bool AutoDeleteConversations { get; set; }
    public int AutoDeleteConversationsInterval { get; set; } = 300000;
    public int DeleteConversationIfInactiveFor { get; set; } = -1;
    public string SessionToken { get; set; }
    public Account Account { get; set; }
}