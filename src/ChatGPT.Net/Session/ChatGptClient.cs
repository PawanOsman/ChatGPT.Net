using System.Text;
using ChatGPT.Net.DTO;
using ChatGPT.Net.Enums;
using Newtonsoft.Json;
using SocketIOClient;

namespace ChatGPT.Net.Session;

public class ChatGptClient
{
    private bool LocalReady { get; set; }
    public string SessionToken { get; set; }
    private string AccessToken { get; set; }
    private AccountType AccountType { get; set; }
    private DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public Action<string> OnError;
    private Func<bool> GetReadyState { get; set; }
    private Func<SocketIO> GetSocketConnection { get; set; }
    public List<ChatGptConversation> Conversations { get; set; }

    public ChatGptClient(ChatGptClientConfig clientConfig, Func<bool> getReadyState, Func<SocketIO> getSocketConnection)
    {
        SessionToken = clientConfig.SessionToken;
        AccountType = clientConfig.AccountType;
        GetReadyState = getReadyState;
        GetSocketConnection = getSocketConnection;
        Conversations = new List<ChatGptConversation>()
        {
            new()
            {
                Id = "default",
                ParentMessageId = Guid.NewGuid().ToString()
            }
        };

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
        var socket = GetSocketConnection();
        await socket.EmitAsync("getSession", async (response) =>
        {
            var result = response.GetValue<SessionInfo>();
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                OnError?.Invoke(result.Error);
                Console.WriteLine(result.Error);
            }
            SessionToken = result.SessionToken;
            AccessToken = result.Auth;
            LocalReady = true;
        }, SessionToken);
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

        var socket = GetSocketConnection();
        var chatGptRequest = new ChatGptRequest()
        {
            Prompt = prompt,
            ParentId = conversation.ParentMessageId,
            ConversationId = conversation.ConversationId,
            Auth = AccessToken
        };
        var tcs = new TaskCompletionSource<string>();
        switch (AccountType)
        {
            case AccountType.Free:
                await socket.EmitAsync("askQuestion", (response) =>
                {
                    var result = response.GetValue<ChatGptResponse>();
                    if (!string.IsNullOrWhiteSpace(result.Error))
                    {
                        OnError?.Invoke(result.Error);
                        Console.WriteLine(result.Error);
                    }
                    conversation.ConversationId = result.ConversationId;
                    conversation.ParentMessageId = result.MessageId;
                    conversation.Updated = DateTime.Now;
                    tcs.SetResult(result.Answer);
                }, chatGptRequest);
                break;
            case AccountType.Pro:
                await socket.EmitAsync("askQuestionPro", (response) =>
                {
                    var result = response.GetValue<ChatGptResponse>();
                    if (!string.IsNullOrWhiteSpace(result.Error))
                    {
                        OnError?.Invoke(result.Error);
                        Console.WriteLine(result.Error);
                    }
                    conversation.ConversationId = result.ConversationId;
                    conversation.ParentMessageId = result.MessageId;
                    conversation.Updated = DateTime.Now;
                    tcs.SetResult(result.Answer);
                }, chatGptRequest);
                break;
            default:
                tcs.SetResult("Invalid account type");
                break;
        }
        return await tcs.Task;
    }

    public void ResetConversation(string conversationId = "default")
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);
        if (conversation == null) return;
        conversation.ConversationId = null;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
    }
}