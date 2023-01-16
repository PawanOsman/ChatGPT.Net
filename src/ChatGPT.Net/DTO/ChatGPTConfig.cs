namespace ChatGPT.Net.DTO;

public class ChatGptConfig
{
    public bool UseCache { get; set; } = true;
    public bool SaveCache { get; set; } = true;
    public string BypassNode { get; set; } = "https://gpt.pawan.krd";
}