namespace ChatGPT.Net.DTO;

public class ChatGptConfig
{
    public string SessionToken { get; set; }
    public string UserAgent { get; set; }
    public string CfClearance { get; set; }
    public string AccessToken { get; set; }
    public Account Account { get; set; }
    public string DataDir { get; set; }
    public bool Incognito { get; set; }
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> Proxies { get; set; } = new Dictionary<string, string>();
}