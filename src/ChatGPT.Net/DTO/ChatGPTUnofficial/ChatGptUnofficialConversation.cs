namespace ChatGPT.Net.DTO.ChatGPTUnofficial;

public class ChatGptUnofficialConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public string? ConversationId { get; set; }
    public string ParentMessageId { get; set; } = Guid.NewGuid().ToString();
}