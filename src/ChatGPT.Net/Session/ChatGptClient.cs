using System.Net.Http.Headers;
using System.Text;
using ChatGPT.Net.DTO;
using Newtonsoft.Json;
using SocketIOClient;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatGPT.Net.Session;

public class ChatGptClient
{
    public bool AutoDeleteConversations { get; set; }
    public int AutoDeleteConversationsInterval { get; set; } = 300000;
    public int DeleteConversationIfInactiveFor { get; set; } = -1;
    public bool LocalReady { get; set; }
    public bool BrowserMode { get; set; }
    public string SessionToken { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public Account Account { get; set; }
    public Func<string> GetUserAgent { get; set; }
    public Func<string> GetCfClearance { get; set; }
    public Func<bool> GetReadyState { get; set; }
    public Func<SocketIO> GetSocketConnection { get; set; }
    public List<ChatGptConversation> Conversations { get; set; }
    private Func<string, string, string, string, string, Task<Reply>> SendMessage { get; set; }
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    public ChatGptClient(bool browserMode, ChatGptClientConfig clientConfig, Func<string, string, string, string, string, Task<Reply>> SendMessageFunc, Func<string> getUserAgent, Func<string> getCfClearance, Func<bool> getReadyState, Func<SocketIO> getSocketConnection)
    {
        AutoDeleteConversations = clientConfig.AutoDeleteConversations;
        AutoDeleteConversationsInterval = clientConfig.AutoDeleteConversationsInterval;
        DeleteConversationIfInactiveFor = clientConfig.DeleteConversationIfInactiveFor;
        SendMessage = SendMessageFunc;
        SessionToken = clientConfig.SessionToken;
        Account = clientConfig.Account;
        GetUserAgent = getUserAgent;
        GetCfClearance = getCfClearance;
        GetReadyState = getReadyState;
        BrowserMode = browserMode;
        GetSocketConnection = getSocketConnection;
        Conversations = new List<ChatGptConversation>()
        {
            new()
            {
                Id = "default",
                ParentMessageId = Guid.NewGuid().ToString()
            }
        };

        if (DeleteConversationIfInactiveFor != -1)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var conversations = Conversations.Where(x => DateTime.Now - x.Updated > TimeSpan.FromMilliseconds(DeleteConversationIfInactiveFor)).ToList();
                    foreach (var conversation in conversations)
                    {
                        await DeleteConversationById(conversation.Id);
                        await Task.Delay(1000);
                    }
                    await Task.Delay(DeleteConversationIfInactiveFor);
                }
            });
        }

        if (AutoDeleteConversations)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await DeleteAllConversations();
                    await Task.Delay(AutoDeleteConversationsInterval);
                }
            });
        }
        
        Task.Run(async () =>
        {
            while (true)
            {
                if(!ValidateAccessToken())
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
        LocalReady = false;
        if (BrowserMode)
        {
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
            LocalReady = true;
        }
        else
        {
            var socket = GetSocketConnection();
            await socket.EmitAsync("getSession", async (response) =>
            {
                var result = response.GetValue<SessionInfo>();
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    throw new Exception(result.Error);
                }
                SessionToken = result.SessionToken;
                AccessToken = result.Auth;
                ExpiresAt = result.Expires;
                LocalReady = true;
            }, SessionToken);
        }
    }

    private async Task WaitForReady()
    {
        while (!GetReadyState()) await Task.Delay(25);
    }

    private async Task WaitForLocalReady()
    {
        while (!LocalReady) await Task.Delay(25);
    }

    private bool ValidateAccessToken()
    {
        if (string.IsNullOrEmpty(AccessToken)) return false;
        if (AccessTokenExpiresAt is null)
        {
            var base64Url = AccessToken.Split('.')[1];
            var base64 = $"{base64Url.Replace('-', '+').Replace('_', '/')}==";
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var parsed = JsonConvert.DeserializeObject<OpenAiProfile>(json);
            AccessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(parsed.Expires);
        }
        return AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddDays(-1);
    }

    public async Task<string> Ask(string prompt, string conversationId = "default")
    {
        await WaitForReady();
        await WaitForLocalReady();

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation == null)
        {
            conversation = new ChatGptConversation()
            {
                Id = conversationId,
                ParentMessageId = Guid.NewGuid().ToString()
            };
            Conversations.Add(conversation);
        }

        if (BrowserMode)
        {
            var reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, AccessToken);

            if(reply.ConversationId is not null)
            {
                conversation.ConversationId = reply.ConversationId;
            }

            if(reply.Message.Id is not null)
            {
                conversation.ParentMessageId = reply.Message.Id;
            }

            conversation.Updated = DateTime.Now;

            return reply.Message.Content.Parts.FirstOrDefault();
        }

        var socket = GetSocketConnection();
        var chatGptRequest = new ChatGptRequest()
        {
            Prompt = prompt,
            ParentId = conversation.ParentMessageId,
            ConversationId = conversation.ConversationId,
            Auth = AccessToken
        };
        var tcs = new TaskCompletionSource<string>();
        await socket.EmitAsync("askQuestion", async (response) =>
        {
            var result = response.GetValue<ChatGptResponse>();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                throw new Exception(result.Error);
            }
            conversation.ConversationId = result.ConversationId;
            conversation.ParentMessageId = result.MessageId;
            conversation.Updated = DateTime.Now;
            tcs.SetResult(result.Answer);
        }, chatGptRequest);
        return await tcs.Task;
    }

    public void ResetConversation(string conversationId = "default")
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);
        if (conversation == null) return;
        conversation.ConversationId = null;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
    }

    // Method by shêr#0196 https://github.com/optionsx
    public async Task DeleteAllConversations()
    {
        if(!BrowserMode)
            throw new Exception("DeleteAllConversations is only available in browser mode.");
        await WaitForReady();
        await WaitForLocalReady();

        Conversations = new List<ChatGptConversation>()
        {
            new()
            {
                Id = "default",
                ParentMessageId = Guid.NewGuid().ToString()
            }
        };

        var client = new HttpClient();

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), "https://chat.openai.com/backend-api/conversations");

        request.Headers.Add("User-Agent", GetUserAgent());
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Referer", "https://chat.openai.com/chat");
        request.Headers.Add("Origin", "https://chat.openai.com");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Cookie", $"cf_clearance={GetCfClearance()};");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Sec-Fetch-Mode", "no-cors");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("TE", "trailers");
        request.Headers.Add("Authorization", $"Bearer {AccessToken}");
        request.Headers.Add("Alt-Used", "chat.openai.com");
        request.Headers.Add("Pragma", "no-cache");
        request.Headers.Add("Cache-Control", "no-cache");

        request.Content = new StringContent("{\"is_visible\":\"false\"}");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.SendAsync(request);
    }

    // Method by shêr#0196 https://github.com/optionsx
    public async Task DeleteConversationById(string conversationId)
    {
        if(!BrowserMode)
            throw new Exception("DeleteAllConversations is only available in browser mode.");
        await WaitForReady();
        await WaitForLocalReady();

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);
        
        if(conversation is not null)
        {
            conversationId = conversation.ConversationId;
            Conversations.Remove(conversation);
        }
        
        var client = new HttpClient();

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://chat.openai.com/backend-api/conversation/{conversationId}");

        request.Headers.Add("User-Agent", GetUserAgent());
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Referer", $"https://chat.openai.com/chat/{conversationId}");
        request.Headers.Add("Origin", "https://chat.openai.com");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Cookie", $"cf_clearance={GetCfClearance()};");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Sec-Fetch-Mode", "no-cors");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("TE", "trailers");
        request.Headers.Add("Authorization", $"Bearer {AccessToken}");
        request.Headers.Add("Alt-Used", "chat.openai.com");
        request.Headers.Add("Pragma", "no-cache");
        request.Headers.Add("Cache-Control", "no-cache");

        request.Content = new StringContent("{\"is_visible\":\"false\"}");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await client.SendAsync(request);
    }
}