namespace ChatGPT.Net.DTO;

public class ChatGptConversation
{
    public string Id { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; }
    public string ConversationId { get; set; }
    public string ParentMessageId { get; set; } = Guid.NewGuid().ToString();
}