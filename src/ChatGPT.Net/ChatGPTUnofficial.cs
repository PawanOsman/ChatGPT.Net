using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChatGPT.Net.DTO;
using ChatGPT.Net.DTO.ChatGPT;
using ChatGPT.Net.DTO.ChatGPTUnofficial;
using Newtonsoft.Json;

namespace ChatGPT.Net;

public class ChatGptUnofficial
{
    public Guid SessionId { get; set; }
    public ChatGptUnofficialOptions Config { get; set; } = new();
    public List<ChatGptUnofficialConversation> Conversations { get; set; } = new();
    public string SessionToken { get; set; }
    public string AccessToken { get; set; }

    public ChatGptUnofficial(string sessionToken, ChatGptUnofficialOptions? config = null)
    {
        Config = config ?? new ChatGptUnofficialOptions();
        SessionId = Guid.NewGuid();
        SessionToken = sessionToken;
    }

    public async Task RefreshAccessToken()
    {
        var client = new HttpClient(new HttpClientHandler
        {
            UseCookies = false,
        });
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Config.BaseUrl}/api/auth/session"),
            Headers =
            {
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0"},
                {"Accept", "application/json"},
                {"Cookie", $"__Secure-next-auth.session-token={SessionToken}" }
            }
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ChatGptUnofficialProfile>();

        const string name = "__Secure-next-auth.session-token=";
        var cookies = response.Headers.GetValues("Set-Cookie");
        var sToken = cookies.FirstOrDefault(x => x.StartsWith(name));

        SessionToken = sToken == null ? SessionToken : sToken.Replace(name, "");

        if (content is not null)
        {
            if(content.Error is null) AccessToken = content.AccessToken;
        }
    }

    private async IAsyncEnumerable<string> StreamCompletion(Stream stream)
    {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                yield return line;
            }
        }
    }

    public List<ChatGptUnofficialConversation> GetConversations()
    {
        return Conversations;
    }

    public void SetConversations(List<ChatGptUnofficialConversation> conversations)
    {
        Conversations = conversations;
    }

    public ChatGptUnofficialConversation GetConversation(string? conversationId)
    {
        if (conversationId is null)
        {
            return new ChatGptUnofficialConversation();
        }

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null) return conversation;
        conversation = new ChatGptUnofficialConversation
        {
            Id = conversationId
        };
        Conversations.Add(conversation);

        return conversation;
    }
    
    public void SetConversation(string conversationId, ChatGptUnofficialConversation conversation)
    {
        var conv = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conv != null)
        {
            conv = conversation;
        }
        else
        {
            Conversations.Add(conversation);
        }
    }
    
    public void RemoveConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null)
        {
            Conversations.Remove(conversation);
        }
    }

    public void ResetConversation(string conversationId)
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation == null) return;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
        conversation.ConversationId = null;
    }

    public void ClearConversations()
    {
        Conversations.Clear();
    }

    public async Task<string> Ask(string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        var reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model);

        if(reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if(reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    public async Task<string> AskStream(Action<string> callback, string prompt, string? conversationId = null)
    {
        var conversation = GetConversation(conversationId);

        var reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model,
            response =>
            {
                var content = response.Message.Content.Parts.FirstOrDefault();
                if (content is not null) callback(content);
            });

        if(reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if(reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    private bool ValidateToken(string token) {
        if (string.IsNullOrWhiteSpace(token)) {
            return false;
        }
    
        var tokenParts = token.Split('.');
        if (tokenParts.Length != 3) {
            return false;
        }
        
        //Ensure the string length is a multiple of 4
        var tokenPart = tokenParts[1];
        var remainderLength = tokenPart.Length % 4;
        if (remainderLength > 0)
            tokenPart = tokenPart.PadRight(tokenPart.Length - remainderLength + 4, '=');

        var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(tokenPart));
        var parsed = JsonDocument.Parse(decodedPayload).RootElement;
    
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() <= parsed.GetProperty("exp").GetInt64() * 1000;
    }

    public async Task<ChatGptUnofficialMessageResponse> SendMessage(string message, string messageId, string? parentMessageId = null, string? conversationId = null, string? model = null, Action<ChatGptUnofficialMessageResponse>? callback = null)
    {
        if(!ValidateToken(AccessToken)) await RefreshAccessToken();
        var requestData = new ChatGptUnofficialMessageRequest
        {
            Messages = new List<MessageElement>
            {
                new()
                {
                    Content = new Content
                    {
                        Parts = new List<string>
                        {
                            message
                        }
                    }
                }
            }
        };
 
        if (model is not null)
        {
            requestData.Model = model;
        }
 
        if (conversationId is not null)
        {
            requestData.ConversationId = conversationId;
        }
        
        if (parentMessageId is not null)
        {
            requestData.ParentMessageId = parentMessageId;
        }

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{Config.BaseUrl}/backend-api/conversation"),
            Headers =
            {
                {"Authorization", $"Bearer {AccessToken}" }
            },
            Content = new StringContent(JsonConvert.SerializeObject(requestData))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        
        ChatGptUnofficialMessageResponse? reply = null;

        await foreach (var data in StreamCompletion(stream))
        {
            var dataJson = data;
            //Ignore ping event
            if (dataJson.StartsWith("event: "))
                continue;
            //Trim start
            if (dataJson.StartsWith("data: "))
                dataJson = dataJson[6..];
            //Ignore stream end tag
            if (dataJson.ToLower() == "[done]")
                continue;
            //Try Deserialize
            try
            {
                var replyNew = JsonConvert.DeserializeObject<ChatGptUnofficialMessageResponse>(dataJson);
                if (replyNew == null)
                    continue;
                reply = replyNew;
                callback?.Invoke(reply);
            }
            catch { }
        }

        return reply ?? new ChatGptUnofficialMessageResponse();
    }
}