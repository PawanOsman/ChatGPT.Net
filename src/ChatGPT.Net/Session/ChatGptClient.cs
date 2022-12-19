using ChatGPT.Net.DTO;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatGPT.Net.Session;

public class ChatGptClient
{
    public string SessionToken { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Account Account { get; set; }
    public Func<string> GetUserAgent { get; set; }
    public Func<string> GetCfClearance { get; set; }
    public Func<bool> GetReadyState { get; set; }
    public List<ChatGptConversation> Conversations { get; set; }
    private Func<string, string, string, string, Task<Reply>> SendMessage { get; set; }
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    public ChatGptClient(ChatGptClientConfig clientConfig, Func<string, string, string, string, Task<Reply>> SendMessageFunc, Func<string> getUserAgent, Func<string> getCfClearance, Func<bool> getReadyState)
    {
        SendMessage = SendMessageFunc;
        SessionToken = clientConfig.SessionToken;
        Account = clientConfig.Account;
        GetUserAgent = getUserAgent;
        GetCfClearance = getCfClearance;
        GetReadyState = getReadyState;
        Conversations = new List<ChatGptConversation>()
        {
            new()
            {
                ConversationId = "default",
                ParentMessageId = Guid.NewGuid().ToString()
            }
        };
        Task.Run(async () =>
        {
            while (true)
            {
                if(ExpiresAt - DateTime.Now < TimeSpan.FromMinutes(5))
                {
                    await RefreshAccessToken();
                }
                await Task.Delay(30000);
            }
        });
    }

    public async Task RefreshAccessToken()
    {
        await WaitForReady();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        client.DefaultRequestHeaders.Add("Referer", "https://chat.openai.com/chat");
        client.DefaultRequestHeaders.Add("X-OpenAI-Assistant-App-Id", "");
        client.DefaultRequestHeaders.Add("Origin", "https://chat.openai.com");
        client.DefaultRequestHeaders.Add("DNT", "1");
        client.DefaultRequestHeaders.Add("Alt-Used", "chat.openai.com");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        client.DefaultRequestHeaders.Add("TE", "trailers");
        client.DefaultRequestHeaders.Add("Cookie", $"cf_clearance={GetCfClearance()};__Secure-next-auth.callback-url=https://chat.openai.com/;__Secure-next-auth.session-token={SessionToken}");

        var response = await client.GetAsync("https://chat.openai.com/api/auth/session");

        response.EnsureSuccessStatusCode();
        
        const string name = "__Secure-next-auth.session-token=";
        var cookies = response.Headers.GetValues("Set-Cookie");
        var sToken = cookies.FirstOrDefault(x => x.StartsWith(name));
        SessionToken = sToken == null ? SessionToken : sToken.Substring(name.Length, sToken.IndexOf(";") - name.Length);
        
        var content = await response.Content.ReadAsStringAsync();
        
        var profile = JsonSerializer.Deserialize<Profile>(content, _jsonSerializerOptions);
        AccessToken = profile.AccessToken;
        ExpiresAt = profile.Expires;
    }
    
    private async Task WaitForReady()
    {
        while (!GetReadyState()) await Task.Delay(25);
    }

    public async Task<string> Ask(string prompt, string conversationId = "default")
    {
        await WaitForReady();

        var conversation = Conversations.FirstOrDefault(x => x.ConversationId == conversationId);

        if (conversation == null)
        {
            conversation = new ChatGptConversation()
            {
                ConversationId = conversationId,
                ParentMessageId = Guid.NewGuid().ToString()
            };
            Conversations.Add(conversation);
        }

        var reply = await SendMessage(prompt, conversation.ParentMessageId, conversation.ConversationId, AccessToken);

        conversation.ConversationId = reply.ConversationId;
        conversation.ParentMessageId = reply.Message.Id;

        return reply.Message.Content.Parts.FirstOrDefault();
    }

    public void ResetConversation(string conversationId = "default")
    {
        var conversation = Conversations.FirstOrDefault(x => x.ConversationId == conversationId);
        if (conversation == null) return;
        conversation.ConversationId = null;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
    }
}