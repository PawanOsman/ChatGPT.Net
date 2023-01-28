using System.Text;
using ChatGPT.Net.DTO;
using ChatGPT.Net.Enums;
using Newtonsoft.Json;
using SocketIOClient;

namespace ChatGPT.Net.Session;

public class ChatGptClient
{
    private string Name { get; set; }
    private bool LocalReady { get; set; }
    public string SessionToken { get; set; }
    private string AccessToken { get; set; }
    private string Signature { get; set; }
    private AccountType AccountType { get; set; }
    private DateTimeOffset? AccessTokenExpiresAt { get; set; }
    public Action<string> OnError;
    public string ConfigDir { get; set; }
    public string FilePath { get; set; }
    private Func<bool> GetReadyState { get; set; }
    private Func<SocketIO> GetSocketConnection { get; set; }
    public List<ChatGptConversation> Conversations { get; set; }

    public ChatGptClient(ChatGptClientConfig clientConfig, Func<bool> getReadyState, Func<SocketIO> getSocketConnection)
    {
        Name = clientConfig.Name;
        SessionToken = clientConfig.SessionToken;
        AccountType = clientConfig.AccountType;
        GetReadyState = getReadyState;
        Signature = GetSignature();
        GetSocketConnection = getSocketConnection;
        ConfigDir = clientConfig.ConfigDir ?? "Configs";
        var configDirFullPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigDir);
        if (!Directory.Exists(configDirFullPath)) Directory.CreateDirectory(configDirFullPath);
        Directory.GetFiles(Directory.GetCurrentDirectory())
            .Where(file => file.EndsWith("-ChatGPT-Net.json"))
            .ToList()
            .ForEach(file =>
            {
                File.Move(file, Path.Combine(ConfigDir, Path.GetFileName(file)));
                Console.WriteLine($"Moved {file} to {ConfigDir}");
            });
        FilePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigDir, $"{Name}-ChatGPT-Net.json");
        Conversations = new List<ChatGptConversation>()
        {
            new()
            {
                Id = "default",
                ParentMessageId = Guid.NewGuid().ToString()
            }
        };

        Load().Wait();

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

        Task.Run(async () =>
        {
            while (true)
            {
                await Save();
                await Task.Delay(60000);
            }
        });
    }

    private string GetSignature()
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(SessionToken);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    public async Task Save()
    {
        var configData = new ConfigData
        {
            Name = Name,
            SessionToken = SessionToken,
            AccountType = AccountType,
            AccessTokenExpiresAt = AccessTokenExpiresAt,
            AccessToken = AccessToken,
            Conversations = Conversations,
            Signature = Signature
        };
        
        var configJson = JsonConvert.SerializeObject(configData, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, configJson);
    }

    private async Task Load()
    {
        if (File.Exists(FilePath))
        {
            var configJson = await File.ReadAllTextAsync(FilePath);
            var configData = JsonConvert.DeserializeObject<ConfigData>(configJson);
            if(configData == null)
                return;
            Name = configData.Name;
            AccountType = configData.AccountType;
            AccessTokenExpiresAt = configData.AccessTokenExpiresAt;
            AccessToken = configData.AccessToken;
            Conversations = configData.Conversations;
            await Task.Delay(1000);
            if (configData.Signature != Signature)
            {
                Console.WriteLine("Session token changed, re-authenticating the new session token...");
                AccessToken = null;
                await RefreshAccessToken();
            }
            else
            {
                SessionToken = configData.SessionToken;
            }

            if(AccessToken is null)
                LocalReady = true;
        }
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
            await Save();
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

        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId || x.ConversationId == conversationId);

        if (conversation == null)
        {
            var isOpenAI = Guid.TryParse(conversationId, out _);
            conversation = new ChatGptConversation()
            {
                Id = conversationId,
                ParentMessageId = Guid.NewGuid().ToString()
            };
            if (isOpenAI) conversation.ConversationId = conversationId;
            Conversations.Add(conversation);
            await Save();
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
                    else
                    {
                        conversation.ConversationId = result.ConversationId;
                        conversation.ParentMessageId = result.MessageId;
                        conversation.Updated = DateTime.Now;
                    }
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
                    else
                    {
                        conversation.ConversationId = result.ConversationId;
                        conversation.ParentMessageId = result.MessageId;
                        conversation.Updated = DateTime.Now;
                    }
                    tcs.SetResult(result.Answer);
                }, chatGptRequest);
                break;
            default:
                tcs.SetResult("Invalid account type");
                break;
        }
        return await tcs.Task;
    }

    public async Task ResetConversation(string conversationId = "default")
    {
        var conversation = Conversations.FirstOrDefault(x => x.Id == conversationId);
        if (conversation == null) return;
        conversation.ConversationId = null;
        conversation.ParentMessageId = Guid.NewGuid().ToString();
        await Save();
    }
}