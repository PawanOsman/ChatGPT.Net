namespace ChatGPT.Net.DTO.ChatGPT;

public class ChatGptOptions : ChatGptConfig
{
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string Model { get; set; } = "gpt-3.5-turbo";
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 0.9;
    public long MaxTokens { get; set; } = 256;
    public string[]? Stop { get; set; } = null;
    public double PresencePenalty { get; set; } = 0.0;
    public double FrequencyPenalty { get; set; } = 0.0;
}