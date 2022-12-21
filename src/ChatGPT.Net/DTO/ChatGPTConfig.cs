namespace ChatGPT.Net.DTO;

public class ChatGptConfig
{
    public bool UseCache { get; set; } = true;
    public bool SaveCache { get; set; } = true;
    public bool Invisible { get; set; }
    public string DataDir { get; set; }
    public bool Incognito { get; set; }
}