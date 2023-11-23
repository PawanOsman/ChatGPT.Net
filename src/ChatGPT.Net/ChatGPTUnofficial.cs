using ChatGPT.Net.DTO;
using ChatGPT.Net.DTO.ChatGPTUnofficial;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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
        HttpClient client = ChatGpt.httpClient;
        HttpRequestMessage request = new()
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

        HttpResponseMessage response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        ChatGptUnofficialProfile? content = await response.Content.ReadFromJsonAsync<ChatGptUnofficialProfile>();

        const string name = "__Secure-next-auth.session-token=";
        IEnumerable<string> cookies = response.Headers.GetValues("Set-Cookie");
        string? sToken = cookies.FirstOrDefault(x => x.StartsWith(name));

        SessionToken = sToken == null ? SessionToken : sToken.Replace(name, "");

        if (content is not null)
        {
            if (content.Error is null) AccessToken = content.AccessToken;
        }
    }

    private async IAsyncEnumerable<string> StreamCompletion(Stream stream)
    {
        using StreamReader reader = new(stream);
        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
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

        ChatGptUnofficialConversation? conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

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
        ChatGptUnofficialConversation? conv = Conversations.FirstOrDefault(x => x.Id == conversationId);

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
        ChatGptUnofficialConversation? conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

        if (conversation != null)
        {
            Conversations.Remove(conversation);
        }
    }

    public void ResetConversation(string conversationId)
    {
        ChatGptUnofficialConversation? conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);

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
        ChatGptUnofficialConversation conversation = GetConversation(conversationId);

        ChatGptUnofficialMessageResponse reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model);

        if (reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if (reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    public async Task<string> AskStream(Action<string> callback, string prompt, string? conversationId = null)
    {
        ChatGptUnofficialConversation conversation = GetConversation(conversationId);

        ChatGptUnofficialMessageResponse reply = await SendMessage(prompt, Guid.NewGuid().ToString(), conversation.ParentMessageId, conversation.ConversationId, Config.Model,
            response =>
            {
                string? content = response.Message.Content.Parts.FirstOrDefault();
                if (content is not null) callback(content);
            });

        if (reply.ConversationId is not null)
        {
            conversation.ConversationId = reply.ConversationId;
        }

        if (reply.Message.Id is not null)
        {
            conversation.ParentMessageId = reply.Message.Id;
        }

        conversation.Updated = DateTime.Now;

        return reply.Message.Content.Parts.FirstOrDefault() ?? "";
    }

    private bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string[] tokenParts = token.Split('.');
        if (tokenParts.Length != 3)
        {
            return false;
        }

        //Ensure the string length is a multiple of 4
        string tokenPart = tokenParts[1];
        int remainderLength = tokenPart.Length % 4;
        if (remainderLength > 0)
            tokenPart = tokenPart.PadRight(tokenPart.Length - remainderLength + 4, '=');

        string decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(tokenPart));
        JsonElement parsed = JsonDocument.Parse(decodedPayload).RootElement;

        return DateTimeOffset.Now.ToUnixTimeMilliseconds() <= parsed.GetProperty("exp").GetInt64() * 1000;
    }

    public async Task<ChatGptUnofficialMessageResponse> SendMessage(string message, string messageId, string? parentMessageId = null, string? conversationId = null, string? model = null, Action<ChatGptUnofficialMessageResponse>? callback = null)
    {
        if (!ValidateToken(AccessToken)) await RefreshAccessToken();
        ChatGptUnofficialMessageRequest requestData = new()
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

        HttpClient client = ChatGpt.httpClient;
        HttpRequestMessage request = new()
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

        HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync();

        ChatGptUnofficialMessageResponse? reply = null;

        await foreach (string data in StreamCompletion(stream))
        {
            string dataJson = data;
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
                ChatGptUnofficialMessageResponse? replyNew = JsonConvert.DeserializeObject<ChatGptUnofficialMessageResponse>(dataJson);
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