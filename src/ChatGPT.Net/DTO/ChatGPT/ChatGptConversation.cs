namespace ChatGPT.Net.DTO.ChatGPT;

public class ChatGptConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public List<ChatGptMessage> Messages { get; set; } = new();
}